using Endpointer.Json.Remapping.Domain;
using Endpointer.Json.Remapping.Services.StorageProvider;

namespace Endpointer.Json.Remapping.Services.JsonMapping;

public class JsonMappingRequestPreparar : IJsonMappingRequestPreparar
{
    private readonly IStorageProviderFactory _storageProviderFactory;

    public JsonMappingRequestPreparar(IStorageProviderFactory storageProviderFactory)
    {
        _storageProviderFactory = storageProviderFactory;
    }
    public async Task<(bool isValid, IEnumerable<string>? messages)> PrepareAsync(JsonMappingRequest request)
    {
        if (string.IsNullOrEmpty(request.StorageProvider?.Trim()))
            return (false, ["StorageProvider is required"]);
        var storage = _storageProviderFactory[request.StorageProvider];
        if (storage == null)
            return (false, [$"storage provider {request.StorageProvider} is not supported"]);
        if (string.IsNullOrEmpty(request.FilePath?.Trim()))
            return (false, ["FilePath is required"]);

        request.Bytes = await storage.GetFileStreamAsync(request.FilePath, CancellationToken.None);
        if (request.Bytes == null)
            return (false, [$"File not found at path {request.FilePath}"]);

        return (true, null);
    }
}