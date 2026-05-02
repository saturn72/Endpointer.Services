namespace Endpointer.Json.Remapping.Domain;

public class JsonMappingRequest
{
    public required string Id { get; init; }
    public required string StorageProvider { get; init; }
    public string FilePath { get; init; } = default!;
    internal Stream? Bytes { get; set; }
}