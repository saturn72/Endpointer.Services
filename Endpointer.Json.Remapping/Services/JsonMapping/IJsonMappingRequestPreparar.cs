using Endpointer.Json.Remapping.Domain;

namespace Endpointer.Json.Remapping.Services.JsonMapping;

public interface IJsonMappingRequestPreparar
{
    Task<(bool isValid, IEnumerable<string>? messages)> PrepareAsync(JsonMappingRequest request);
}
