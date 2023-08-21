namespace CelesteMarina.DependencyInjection.Provider
{
    public interface IServiceProviderScopeFactory
    {
        IServiceProviderScope CreateScope();
    }
}