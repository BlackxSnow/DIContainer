using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using DIContainer.Extensions;
using DIContainer.Injector;
using DIContainer.Provider;
using DIContainer.Service;
using Microsoft.Extensions.Logging;

namespace DIContainer.CallSite
{
    internal class CallSiteFactory : ICallSiteFactory
    {
        internal IInjectorCallSiteFactory? InjectorCallSiteFactory { get; set; }
        private Dictionary<ServiceIdentifier, List<ServiceDescriptor>> _Descriptors;
        private Dictionary<ServiceCacheKey, ServiceCallSite> _CallSiteCache;

        private ILogger<CallSiteFactory> _Logger;

        public ServiceCallSite? GetCallSite(ServiceIdentifier identifier)
        {
            var key = new ServiceCacheKey(identifier);
            return _CallSiteCache.TryGetValue(key, out ServiceCallSite callSite) ? callSite : BuildCallSite(identifier);
        }

        private ServiceCallSite? BuildCallSite(ServiceIdentifier identifier)
        {
            return TryBuildExact(identifier) ??
                   TryBuildOpenGeneric(identifier) ??
                   TryBuildEnumerable(identifier);
        }
        
        private ServiceCallSite? TryBuildExact(ServiceIdentifier identifier)
        {
            if (_Descriptors.TryGetValue(identifier, out List<ServiceDescriptor> descriptors))
            {
                return TryBuildExact(descriptors[0], identifier);
            }
            
            return null;
        }

        private ServiceCallSite? TryBuildExact(ServiceDescriptor descriptor, ServiceIdentifier identifier)
        {
            if (descriptor.ServiceType != identifier.ServiceType) return null;

            ServiceCallSite callSite;
            var cacheInfo = new ServiceCacheInfo(descriptor.Lifetime, identifier);
            
            if (descriptor.HasInstance())
            {
                callSite = new ConstantCallSite(descriptor.ServiceType, descriptor.GetInstance()!);
            }
            else if (descriptor.HasFactory())
            {
                callSite = new FactoryCallSite(cacheInfo, descriptor.ServiceType, descriptor.GetFactory()!);
            }
            else if (descriptor.HasImplementationType())
            {
                callSite = BuildConstructorCallSite(cacheInfo, identifier, descriptor.GetImplementationType()!);
            }
            else
            {
                throw new InvalidOperationException(Exceptions.InvalidServiceDescriptor);
            }

            return _CallSiteCache[cacheInfo.CacheKey] = callSite;
        }

        private ServiceCallSite? TryBuildEnumerable(ServiceIdentifier enumerableIdentifier)
        {
            var cacheKey = new ServiceCacheKey(enumerableIdentifier);
            if (_CallSiteCache.TryGetValue(cacheKey, out ServiceCallSite cachedCallSite)) return cachedCallSite;
            if (!enumerableIdentifier.ServiceType.IsConstructedGenericType ||
                enumerableIdentifier.ServiceType.GetGenericTypeDefinition() != typeof(IEnumerable<>)) return null;

            var cacheLocation = CacheLocation.Root;
            Type innerServiceType = enumerableIdentifier.ServiceType.GenericTypeArguments[0];
            var innerIdentifier = new ServiceIdentifier(innerServiceType);

            ServiceCallSite[] callSites;

            if (innerServiceType.IsConstructedGenericType)
            {
                bool hasExact = _Descriptors.TryGetValue(innerIdentifier, out List<ServiceDescriptor> exactDescriptors);
                ServiceIdentifier genericIdentifier = innerIdentifier.GetGenericTypeDefinition();
                bool hasOpenGeneric =
                    _Descriptors.TryGetValue(genericIdentifier, out List<ServiceDescriptor> genericDescriptors);
                int descriptorCount = (exactDescriptors?.Count ?? 0) + (genericDescriptors?.Count ?? 0);
                if (descriptorCount == 0) return null;

                callSites = new ServiceCallSite[descriptorCount];
                var currentIndex = 0;
                if (hasExact)
                {
                    foreach (ServiceDescriptor descriptor in exactDescriptors!)
                    {
                        ServiceCallSite? exactCallSite = TryBuildExact(descriptor, innerIdentifier);
                        Debug.Assert(exactCallSite != null);
                        cacheLocation = CacheUtil.GetCommonCacheLocation(cacheLocation, exactCallSite!.CacheInfo.Location);
                        callSites[currentIndex] = exactCallSite;
                        currentIndex++;
                    }
                }

                if (hasOpenGeneric)
                {
                    foreach (ServiceDescriptor descriptor in genericDescriptors!)
                    {
                        ServiceCallSite? genericCallSite = TryBuildOpenGeneric(genericIdentifier);
                        Debug.Assert(genericCallSite != null);
                        cacheLocation =
                            CacheUtil.GetCommonCacheLocation(cacheLocation, genericCallSite!.CacheInfo.Location);
                        callSites[currentIndex] = genericCallSite;
                        currentIndex++;
                    }
                }
            }
            else if (_Descriptors.TryGetValue(innerIdentifier, out List<ServiceDescriptor> descriptors))
            {
                callSites = new ServiceCallSite[descriptors.Count];
                for (var i = 0; i < descriptors.Count; i++)
                {
                    cachedCallSite = TryBuildExact(descriptors[i], innerIdentifier)!;
                    Debug.Assert(callSites[i] != null);
                    cacheLocation = CacheUtil.GetCommonCacheLocation(cacheLocation, cachedCallSite.CacheInfo.Location);
                    callSites[i] = cachedCallSite;
                }
            }
            else
            {
                return null;
            }

            ServiceCacheInfo cacheInfo = cacheLocation is CacheLocation.Scope or CacheLocation.Root
                ? new ServiceCacheInfo(cacheLocation, cacheKey)
                : new ServiceCacheInfo(CacheLocation.None, cacheKey);
            return _CallSiteCache[cacheKey] = new EnumerableCallSite(cacheInfo, innerServiceType, callSites);
        }

        private ServiceCallSite? TryBuildOpenGeneric(ServiceIdentifier identifier)
        {
            if (!identifier.ServiceType.IsConstructedGenericType) return null;

            ServiceIdentifier genericIdentifier = identifier.GetGenericTypeDefinition();
            if (_Descriptors.TryGetValue(genericIdentifier, out List<ServiceDescriptor> descriptors))
            {
                return TryBuildOpenGeneric(descriptors[0], identifier);
            }

            return null;
        }

        private ServiceCallSite? TryBuildOpenGeneric(ServiceDescriptor descriptor, ServiceIdentifier identifier)
        {
            if (!identifier.ServiceType.IsConstructedGenericType ||
                identifier.ServiceType.GetGenericTypeDefinition() != descriptor.ServiceType)
            {
                return null;
            }

            var cacheKey = new ServiceCacheKey(identifier);
            if (_CallSiteCache.TryGetValue(cacheKey, out ServiceCallSite callSite))
            {
                return callSite;
            }

            Type? implementationType = descriptor.GetImplementationType();
            Debug.Assert(implementationType != null);

            Type[] genericArguments = identifier.ServiceType.GetGenericArguments();
            Type constructedType = implementationType!.MakeGenericType(genericArguments);

            var cacheInfo = new ServiceCacheInfo(descriptor.Lifetime, identifier);
            return _CallSiteCache[cacheKey] = BuildConstructorCallSite(cacheInfo, identifier, constructedType);
            
        }
        
        private ConstructorCallSite BuildConstructorCallSite(ServiceCacheInfo cacheInfo, ServiceIdentifier identifier, Type implementationType)
        {
            _Logger.LogDebug("Building constructor CallSite for {ServiceTypeName}", identifier.ServiceType.Name);
            ConstructorInfo[] constructors = implementationType.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException(string.Format(Exceptions.NoConstructor, implementationType));
            }

            Array.Sort(constructors, (a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));

            ConstructorInfo? bestConstructor = null;
            ServiceCallSite[]? bestParameterCallSites = null;
            
            foreach (ConstructorInfo constructor in constructors)
            {
                ServiceCallSite[]? parameterCallSites = BuildParameterCallSites(constructor.GetParameters());
                if (parameterCallSites == null) continue;
                bestConstructor = constructor;
                bestParameterCallSites = parameterCallSites;
                break;
            }

            if (bestConstructor == null)
            {
                throw new InvalidOperationException(string.Format(Exceptions.NoValidConstructor, implementationType));
            }
            Debug.Assert(bestParameterCallSites != null);
            InjectorCallSite? injectorCallSite = InjectorCallSiteFactory?.GetCallSite(implementationType);
            return new ConstructorCallSite(cacheInfo, injectorCallSite, identifier.ServiceType, bestConstructor, bestParameterCallSites!);
        }

        private ServiceCallSite[]? BuildParameterCallSites(ParameterInfo[] parameters)
        {
            var callSites = new ServiceCallSite[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                ServiceCallSite? callSite = GetCallSite(new ServiceIdentifier(parameter.ParameterType));
                if (callSite == null) return null;
                callSites[i] = callSite;
            }

            return callSites;
        }
        
        private void AddServices(IEnumerable<ServiceDescriptor> services)
        {
            foreach (ServiceDescriptor descriptor in services)
            {
                if (descriptor.ServiceType.IsGenericTypeDefinition) ValidateOpenGenericDescriptor(descriptor);
                else if (!descriptor.HasInstance() && !descriptor.HasFactory())
                {
                    Type? implementationType = descriptor.GetImplementationType();
                    Debug.Assert(implementationType != null);

                    if (!implementationType!.IsConstructable())
                    {
                        throw new ArgumentException(string.Format(Exceptions.TypeCannotBeConstructed,
                            descriptor.ImplementationType, descriptor.ServiceType));
                    }
                }

                var identifier = new ServiceIdentifier(descriptor);
                if (!_Descriptors.TryGetValue(identifier, out List<ServiceDescriptor> descriptors))
                {
                    descriptors = new List<ServiceDescriptor>();
                    _Descriptors.Add(identifier, descriptors);
                }
                descriptors.Add(descriptor);
            }
            _Logger.LogDebug("Added services");
        }

        private void ValidateOpenGenericDescriptor(ServiceDescriptor descriptor)
        {
            Debug.Assert(descriptor.ServiceType.IsGenericTypeDefinition);
            Type? implementationType = descriptor.GetImplementationType();

            if (implementationType is not { IsGenericTypeDefinition: true })
            {
                throw new ArgumentException(string.Format(
                    Exceptions.OpenGenericServiceRequiresOpenGenericImplementation, descriptor.ServiceType,
                    implementationType));
            }

            if (implementationType.IsAbstract || implementationType.IsInterface)
            {
                throw new ArgumentException(string.Format(Exceptions.TypeCannotBeConstructed, implementationType,
                    descriptor.ServiceType));
            }

            if (implementationType.GetGenericArguments().Length != descriptor.ServiceType.GetGenericArguments().Length)
            {
                throw new ArgumentException(string.Format(Exceptions.GenericParameterCountServiceImplementationNotEqual,
                    descriptor.ServiceType, implementationType));
            }
        }
        
        public CallSiteFactory(IEnumerable<ServiceDescriptor> descriptors, ILogger<CallSiteFactory> logger)
        {
            _Logger = logger;
            _Descriptors = new Dictionary<ServiceIdentifier, List<ServiceDescriptor>>();
            _CallSiteCache = new Dictionary<ServiceCacheKey, ServiceCallSite>();
            AddServices(descriptors);
        }
    }
}