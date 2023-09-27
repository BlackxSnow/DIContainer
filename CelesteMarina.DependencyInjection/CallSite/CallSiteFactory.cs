using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Service;
using CelesteMarina.DependencyInjection.Extensions;
using CelesteMarina.DependencyInjection.Provider;
using Microsoft.Extensions.Logging;
using IServiceProvider = CelesteMarina.DependencyInjection.Provider.IServiceProvider;

namespace CelesteMarina.DependencyInjection.CallSite
{
    internal class CallSiteFactory : ICallSiteFactory
    {
        internal IInjectorCallSiteFactory? InjectorCallSiteFactory { get; set; }
        private readonly Dictionary<ServiceIdentifier, List<ServiceDescriptor>> _Descriptors;
        private readonly Dictionary<ServiceCacheKey, ServiceCallSite> _CallSiteCache;
        private readonly Dictionary<ServiceCacheKey, ServiceCallSite> _DeactivatedCache;

        private ILogger? _Logger;

        public ServiceCallSite? GetCallSite(ServiceIdentifier identifier)
        {
            var key = new ServiceCacheKey(identifier);
            return _CallSiteCache.TryGetValue(key, out ServiceCallSite callSite) ? callSite : BuildCallSite(identifier);
        }

        public bool IsService(Type type)
        {
            var identifier = new ServiceIdentifier(type);
            return _Descriptors.ContainsKey(identifier);
        }
        
        private ServiceCallSite? BuildCallSite(ServiceIdentifier identifier)
        {
            return TryBuildExact(identifier) ??
                   TryBuildOpenGeneric(identifier) ??
                   TryBuildEnumerable(identifier);
        }
        
        /// <summary>
        /// Attempt to retrieve the descriptor for, and build a <see cref="ServiceCallSite"/> for the exact requested type.
        /// </summary>
        /// <param name="identifier"><inheritdoc cref="ICallSiteFactory.GetCallSite"/></param>
        /// <returns><inheritdoc cref="ICallSiteFactory.GetCallSite"/></returns>
        private ServiceCallSite? TryBuildExact(ServiceIdentifier identifier)
        {
            if (_Descriptors.TryGetValue(identifier, out List<ServiceDescriptor> descriptors))
            {
                return TryBuildExact(descriptors[0], identifier);
            }
            
            return null;
        }

        /// <summary>
        /// Attempt to build a <see cref="ServiceCallSite"/> for the service described by <paramref name="descriptor"/>.
        /// </summary>
        /// <param name="descriptor">The descriptor for the requested service.</param>
        /// <param name="identifier"><inheritdoc cref="ICallSiteFactory.GetCallSite"/></param>
        /// <returns><inheritdoc cref="ICallSiteFactory.GetCallSite"/></returns>
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

        /// <summary>
        /// Attempt to build an <see cref="EnumerableCallSite"/> of services matching the inner type of
        /// <paramref name="enumerableIdentifier"/>, assuming it is a constructed <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="enumerableIdentifier"><inheritdoc cref="ICallSiteFactory.GetCallSite"/></param>
        /// <returns><inheritdoc cref="ICallSiteFactory.GetCallSite"/></returns>
        /// <remarks>Method will fail if <paramref name="enumerableIdentifier.ServiceType"/> is not a constructed <see cref="IEnumerable{T}"/>.</remarks>
        private ServiceCallSite? TryBuildEnumerable(ServiceIdentifier enumerableIdentifier)
        {
            var cacheKey = new ServiceCacheKey(enumerableIdentifier);
            if (_CallSiteCache.TryGetValue(cacheKey, out ServiceCallSite cachedCallSite)) return cachedCallSite;
            if (!enumerableIdentifier.ServiceType.IsConstructedGenericType ||
                enumerableIdentifier.ServiceType.GetGenericTypeDefinition() != typeof(IEnumerable<>)) return null;

            var cacheLocation = CacheLocation.Root;
            Type innerServiceType = enumerableIdentifier.ServiceType.GenericTypeArguments[0];
            var innerIdentifier = new ServiceIdentifier(innerServiceType);

            ServiceCallSite[]? callSites = innerServiceType.IsConstructedGenericType ? 
                BuildCallSitesForConstructedGeneric(innerIdentifier, ref cacheLocation) :
                BuildCallSitesForExact(innerIdentifier, ref cacheLocation);
            if (callSites == null) return null;

            ServiceCacheInfo cacheInfo = cacheLocation is CacheLocation.Scope or CacheLocation.Root
                ? new ServiceCacheInfo(cacheLocation, cacheKey)
                : new ServiceCacheInfo(CacheLocation.None, cacheKey);
            return _CallSiteCache[cacheKey] = new EnumerableCallSite(cacheInfo, innerServiceType, callSites);
        }

        
        /// <summary>
        /// Retrieve all call sites matching the constructed generic <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier">An identifier representing a constructed generic service type.</param>
        /// <param name="cacheLocation">The broadest common cache location for the retrieved services.</param>
        /// <returns>All valid call sites for <paramref name="identifier"/>, or null if none.</returns>
        /// <remarks>The final value of <paramref name="cacheLocation"/> will never be broader than the input value.</remarks>
        private ServiceCallSite[]? BuildCallSitesForConstructedGeneric(ServiceIdentifier identifier, ref CacheLocation cacheLocation)
        {
            bool hasExact = _Descriptors.TryGetValue(identifier, out List<ServiceDescriptor> exactDescriptors);
            ServiceIdentifier genericIdentifier = identifier.GetGenericTypeDefinition();
            bool hasOpenGeneric =
                _Descriptors.TryGetValue(genericIdentifier, out List<ServiceDescriptor> genericDescriptors);
            int descriptorCount = (exactDescriptors?.Count ?? 0) + (genericDescriptors?.Count ?? 0);
            if (descriptorCount == 0) return null;

            var callSites = new ServiceCallSite[descriptorCount];
                
            var currentIndex = 0;
            if (hasExact)
            {
                foreach (ServiceDescriptor descriptor in exactDescriptors!)
                {
                    ServiceCallSite? exactCallSite = TryBuildExact(descriptor, identifier);
                    Debug.Assert(exactCallSite != null);
                    cacheLocation = CacheUtil.GetCommonCacheLocation(cacheLocation, exactCallSite!.CacheInfo.Location);
                    callSites[currentIndex] = exactCallSite;
                    currentIndex++;
                }
            }

            if (!hasOpenGeneric) return callSites;
            
            foreach (ServiceDescriptor _ in genericDescriptors!)
            {
                ServiceCallSite? genericCallSite = TryBuildOpenGeneric(genericIdentifier);
                Debug.Assert(genericCallSite != null);
                cacheLocation =
                    CacheUtil.GetCommonCacheLocation(cacheLocation, genericCallSite!.CacheInfo.Location);
                callSites[currentIndex] = genericCallSite;
                currentIndex++;
            }

            return callSites;
        }
        
        /// <summary>
        /// Retrieve all call sites matching <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier">An identifier representing the desired service.</param>
        /// <param name="cacheLocation"><inheritdoc cref="BuildCallSitesForConstructedGeneric"/></param>
        /// <returns><inheritdoc cref="BuildCallSitesForConstructedGeneric"/></returns>
        /// <remarks><inheritdoc cref="BuildCallSitesForConstructedGeneric"/></remarks>
        private ServiceCallSite[]? BuildCallSitesForExact(ServiceIdentifier identifier, ref CacheLocation cacheLocation)
        {
            if (!_Descriptors.TryGetValue(identifier, out List<ServiceDescriptor> descriptors)) return null;
            
            var callSites = new ServiceCallSite[descriptors.Count];
            for (var i = 0; i < descriptors.Count; i++)
            {
                ServiceCallSite callSite = TryBuildExact(descriptors[i], identifier)!;
                Debug.Assert(callSites[i] != null);
                cacheLocation = CacheUtil.GetCommonCacheLocation(cacheLocation, callSite.CacheInfo.Location);
                callSites[i] = callSite;
            }

            return callSites;
        }
        
        /// <summary>
        /// Attempt to get an open generic type from <paramref name="identifier"/>, retrieve a descriptor for the open
        /// generic service, and build a <see cref="ServiceCallSite"/> for the inner type of <paramref name="identifier"/>. 
        /// </summary>
        /// <param name="identifier"><inheritdoc cref="ICallSiteFactory.GetCallSite"/></param>
        /// <returns><inheritdoc cref="ICallSiteFactory.GetCallSite"/></returns>
        /// <remarks>Method will fail if <paramref name="identifier.ServiceType"/> is not a constructed generic type.</remarks>
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

        /// <summary>
        /// Attempt to build a <see cref="ServiceCallSite"/> for <paramref name="identifier">identifier's</paramref> inner
        /// type, using an open generic <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="descriptor">The open generic descriptor to construct the call site from.</param>
        /// <param name="identifier"><inheritdoc cref="ICallSiteFactory.GetCallSite"/></param>
        /// <returns><inheritdoc cref="ICallSiteFactory.GetCallSite"/></returns>
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
            _Logger?.LogDebug("Building constructor CallSite for {ServiceTypeName}", identifier.ServiceType.Name);
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
        
        public void AddServices(IEnumerable<ServiceDescriptor> services)
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
                var cacheKey = new ServiceCacheKey(identifier);
                
                if (!_DeactivatedCache.TryGetValue(cacheKey, out ServiceCallSite? cached)) continue;
                cached.IsDisabled = false;
                _CallSiteCache.Add(cacheKey, cached);
                _DeactivatedCache.Remove(cacheKey);
            }
            _Logger?.LogDebug("Added services");
        }

        public void AddService(ServiceIdentifier identifier, ServiceCallSite callSite)
        {
            _CallSiteCache[new ServiceCacheKey(identifier)] = callSite;
        }

        public void RemoveServices(IEnumerable<ServiceDescriptor> services)
        {
            foreach (ServiceDescriptor descriptor in services)
            {
                var identifier = new ServiceIdentifier(descriptor);
                if (!_Descriptors.TryGetValue(identifier, out List<ServiceDescriptor> descriptors)) continue;

                var key = new ServiceCacheKey(identifier);

                if (_CallSiteCache.TryGetValue(key, out ServiceCallSite? cached))
                {
                    cached.IsDisabled = true;
                    _CallSiteCache.Remove(key);
                    _DeactivatedCache.Add(key, cached);
                }
                
                if (descriptors.Count <= 1)
                {
                    _Descriptors.Remove(identifier);
                    continue;
                }

                descriptors.Remove(descriptor);
            }
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

        public void OnInitialisationComplete(IServiceProvider provider)
        {
            _Logger = provider.GetService<ILogger<CallSiteFactory>>();
        }
        
        public CallSiteFactory(IEnumerable<ServiceDescriptor> descriptors, ILogger<CallSiteFactory> logger)
        {
            _Logger = logger;
            _Descriptors = new Dictionary<ServiceIdentifier, List<ServiceDescriptor>>();
            _CallSiteCache = new Dictionary<ServiceCacheKey, ServiceCallSite>();
            _DeactivatedCache = new Dictionary<ServiceCacheKey, ServiceCallSite>();
            AddServices(descriptors);
        }
    }
}