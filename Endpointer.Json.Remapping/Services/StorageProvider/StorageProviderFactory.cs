namespace Endpointer.Json.Remapping.Services.StorageProvider;

public class StorageProviderFactory : IStorageProviderFactory
{
    private readonly IReadOnlyDictionary<string, IStorageProvider> _providers;

    public StorageProviderFactory(IDictionary<string, IStorageProvider> providers)
    {
        _providers = providers.AsReadOnly();
    }

    public IStorageProvider this[string providerName]
    {
        get
        {
            if (_providers.TryGetValue(providerName, out var provider))
                return provider;

            throw new KeyNotFoundException($"Storage provider '{providerName}' is not registered.");
        }
    }
}
