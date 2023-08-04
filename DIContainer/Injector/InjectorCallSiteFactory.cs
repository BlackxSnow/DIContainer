using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DIContainer.CallSite;
using DIContainer.Service;
using Microsoft.Extensions.Logging;

namespace DIContainer.Injector
{
    internal class InjectorCallSiteFactory : IInjectorCallSiteFactory
    {
        private Dictionary<Type, InjectorCallSite> _CallSiteCache;

        private ICallSiteFactory _CallSiteFactory;
        private ILogger<InjectorCallSiteFactory> _Logger;

        public InjectorCallSite GetCallSite(Type type)
        {
            return _CallSiteCache.TryGetValue(type, out InjectorCallSite callSite) ? callSite : BuildCallSite(type);
        }
        
        private InjectorCallSite BuildCallSite(Type type)
        {
            var callSite = new InjectorCallSite(type, GetMethodInjectionPoint(type), GetPropertyInjectionPoints(type));
            _CallSiteCache[type] = callSite;
            return callSite;
        }
        
        private MethodInjectionPoint? GetMethodInjectionPoint(Type type)
        {
            MethodInfo[] injectionMethods =
                type.GetMethods().Where(m => m.GetCustomAttribute(typeof(InjectionAttribute)) != null).ToArray();
            if (injectionMethods.Length == 0) return null;
            if (injectionMethods.Length > 1)
            {
                throw new InvalidOperationException(
                    string.Format(Exceptions.MultipleInjectionMethodsNotSupported, type));
            }

            MethodInfo method = injectionMethods.First();
            ParameterInfo[] parameters = method.GetParameters();
            var parameterCallSites = new ServiceCallSite?[parameters.Length];
            
            for(var i = 0; i < parameters.Length; i++)
            {
                var parameterServiceIdentifier = new ServiceIdentifier(parameters[i].ParameterType);
                ServiceCallSite? parameterCallSite = _CallSiteFactory.GetCallSite(parameterServiceIdentifier);
                parameterCallSites[i] = parameterCallSite;
            }

            return new MethodInjectionPoint(method, parameterCallSites);
        }
        
        private PropertyInjectionPoint[]? GetPropertyInjectionPoints(Type type)
        {
            PropertyInfo[] injectionProperties =
                type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.GetCustomAttribute<InjectionAttribute>() != null).ToArray();

            if (injectionProperties.Length == 0) return null;
            
            var injectionPoints = new PropertyInjectionPoint?[injectionProperties.Count()];

            for (var i = 0; i < injectionProperties.Length; i++)
            {
                PropertyInfo property = injectionProperties[i];
                var propertyIdentifier = new ServiceIdentifier(property.PropertyType);
                ServiceCallSite? propertyCallSite = _CallSiteFactory.GetCallSite(propertyIdentifier);
                if (property.SetMethod == null)
                {
                    throw new InvalidOperationException(
                        string.Format(Exceptions.InjectionPropertyNoSetter, property.Name, type));
                }
                injectionPoints[i] = new PropertyInjectionPoint(injectionProperties[i], propertyCallSite);
            }

            return injectionPoints;
        }

        public InjectorCallSiteFactory(ILogger<InjectorCallSiteFactory> logger, ICallSiteFactory callSiteFactory)
        {
            _Logger = logger;
            _CallSiteCache = new Dictionary<Type, InjectorCallSite>();
            _CallSiteFactory = callSiteFactory;
        }
    }
}