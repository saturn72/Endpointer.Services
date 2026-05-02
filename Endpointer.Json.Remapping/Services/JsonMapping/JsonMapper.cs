using Endpointer.Json.Remapping.Domain;

namespace Endpointer.Json.Remapping.Services.JsonMapping;

public class JsonMapper : IJsonMapper
{
    public async Task MapAsync(JsonMappingRequest request)
    {

        throw new NotImplementedException("Mapping logic not implemented yet. This should be where the request is sent to the appropriate storage provider based on the request's StorageProvider property.");
    }
}