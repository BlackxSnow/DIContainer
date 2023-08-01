using System;

namespace DIContainer.NotableTypes
{
    public interface INotableTypeProvider
    {
        void RegisterListener(Type type, Action<object> listener);
        void RemoveListener(Type type, Action<object> listener);
    }
}