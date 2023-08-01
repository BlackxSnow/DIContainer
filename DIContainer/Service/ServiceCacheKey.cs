namespace DIContainer.Service
{
    public struct ServiceCacheKey
    {
        public ServiceIdentifier Identifier { get; }

        public ServiceCacheKey(ServiceIdentifier identifier)
        {
            Identifier = identifier;
        }
    }
}