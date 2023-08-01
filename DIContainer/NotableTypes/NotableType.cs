using System;

namespace DIContainer.NotableTypes
{
    public class NotableType
    {
        public Type Type { get; }

        private event Action<object> _TypeResolved;

        public void AddListener(Action<object> listener)
        {
            _TypeResolved += listener;
        }

        public void RemoveListener(Action<object> listener)
        {
            _TypeResolved -= listener;
        }

        public void Invoke(object instance)
        {
            _TypeResolved?.Invoke(instance);
        }

        public NotableType(Type type)
        {
            Type = type;
        }
    }
}