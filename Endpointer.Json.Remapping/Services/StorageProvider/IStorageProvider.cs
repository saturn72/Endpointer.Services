namespace Endpointer.Json.Remapping.Services.StorageProvider;

public interface IStorageProvider
{
    Task<Stream> GetFileStreamAsync(string valetKey, CancellationToken cancellationToken);
}