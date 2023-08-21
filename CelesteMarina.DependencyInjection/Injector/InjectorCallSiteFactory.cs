using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.Service;
using Microsoft.Extensions.Logging;

namespace CelesteMarina.DependencyInjection.Injector
{
    /// <summary>
    /// <inheritdoc cref="IInjectorCallSiteFactory"/>
    /// </summary>
    internal class InjectorCallSiteFactory : IInjectorCallSiteFactory
    {
        private Dictionary<Type, InjectorCallSite> _CallSiteCache;

        private ICallSiteFactory _CallSiteFactory;
        private ILogger<InjectorCallSiteFactory> _Logger;

        private const BindingFlags _AllInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public InjectorCallSite GetCallSite(Type type)
        {
            return _CallSiteCache.TryGetValue(type, out InjectorCallSite callSite) ? callSite : BuildCallSite(type);
        }
        
        /// <summary>
        /// Build and cache an <see cref="InjectorCallSite"/> for <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The target type of the call site.</param>
        /// <returns>A call site for the specified type.</returns>
        private InjectorCallSite BuildCallSite(Type type)
        {
            var callSite = new InjectorCallSite(type, GetMethodInjectionPoint(type), GetPropertyInjectionPoints(type));
            _CallSiteCache[type] = callSite;
            return callSite;
        }
        
        /// <summary>
        /// Attempt to build a <see cref="MethodInjectionPoint"/> for the target type.
        /// </summary>
        /// <param name="type"><inheritdoc cref="BuildCallSite"/></param>
        /// <returns>The method injection point for the type, if any.</returns>
        /// <exception cref="InvalidOperationException">Throws on discovery of multiple methods marked with <see cref="InjectionAttribute"/>.</exception>
        private MethodInjectionPoint? GetMethodInjectionPoint(Type type)
        {
            MethodInfo[] injectionMethods =
                type.GetMethods(_AllInstance).Where(m => m.GetCustomAttribute(typeof(InjectionAttribute)) != null)
                    .ToArray();
            switch (injectionMethods.Length)
            {
                case 0:
                    return null;
                case > 1:
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
        
        /// <summary>
        /// Attempt to build the <see cref="PropertyInjectionPoint">PropertyInjectionPoints</see> for the target type.
        /// </summary>
        /// <param name="type"><inheritdoc cref="BuildCallSite"/></param>
        /// <returns>The property injection points for the type, if any.</returns>
        /// <exception cref="InvalidOperationException">Throws on discovery of an injection property with no setter.</exception>
        private PropertyInjectionPoint[]? GetPropertyInjectionPoints(Type type)
        {
            PropertyInfo[] injectionProperties =
                type.GetProperties(_AllInstance)
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