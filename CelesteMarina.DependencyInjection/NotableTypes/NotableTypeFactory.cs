using System;
using System.Collections.Generic;

namespace CelesteMarina.DependencyInjection.NotableTypes
{
    public class NotableTypeFactory
    {
        private Dictionary<Type, NotableType> _NotableTypes;
        private Dictionary<Type, NotableType> _ActiveNotableTypes;
        private Dictionary<Type, List<Type>> _ImplementationNotableTypeCache;

        public IReadOnlyCollection<Type> GetCache(Type type)
        {
            return _ImplementationNotableTypeCache[type];
        }

        public void AddNotableType(Type type)
        {
            var notable = new NotableType(type);
            _NotableTypes.Add(type, notable);
            _ActiveNotableTypes.Add(type, notable);
        }
    }
}