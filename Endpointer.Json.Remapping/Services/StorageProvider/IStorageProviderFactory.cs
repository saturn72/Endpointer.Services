namespace Endpointer.Json.Remapping.Services.StorageProvider;

public interface IStorageProviderFactory
{
    IStorageProvider this[string providerName] { get; }
}
