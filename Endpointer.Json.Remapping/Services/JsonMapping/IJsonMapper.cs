using Endpointer.Json.Remapping.Domain;

namespace Endpointer.Json.Remapping.Services.JsonMapping;

public interface IJsonMapper
{
    Task MapAsync(JsonMappingRequest request);
}
