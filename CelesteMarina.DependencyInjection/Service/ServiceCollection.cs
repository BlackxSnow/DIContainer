using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace CelesteMarina.DependencyInjection.Service
{
    public class ServiceCollection : IServiceCollection
    {
        protected readonly List<ServiceDescriptor> Services = new();

        public int Count => Services.Count;
        public bool IsReadOnly { get; private set; }

        public void MakeReadOnly()
        {
            IsReadOnly = true;
        }
        
        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return Services.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Services.GetEnumerator();
        }

        public void Add(ServiceDescriptor item)
        {
            ThrowIfReadOnly();
            Services.Add(item);
        }

        public void Clear()
        {
            ThrowIfReadOnly();
            Services.Clear();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return Services.Contains(item);
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            Services.CopyTo(array, arrayIndex);
        }

        public bool Remove(ServiceDescriptor item)
        {
            ThrowIfReadOnly();
            return Services.Remove(item);
        }


        public int IndexOf(ServiceDescriptor item)
        {
            return Services.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            ThrowIfReadOnly();
            Services.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ThrowIfReadOnly();
            Services.RemoveAt(index);
        }

        protected void ThrowIfReadOnly()
        {
            if (IsReadOnly)
            {
                throw new ReadOnlyException();
            }
        }
        
        public ServiceDescriptor this[int index]
        {
            get => Services[index];
            set
            {
                ThrowIfReadOnly();
                Services[index] = value;
            }
        }
    }
}