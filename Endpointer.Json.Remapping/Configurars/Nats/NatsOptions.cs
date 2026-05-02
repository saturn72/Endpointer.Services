using System.ComponentModel.DataAnnotations;

namespace Endpointer.Json.Remapping.Configurars;

public class NatsOptions
{
    public const string Section = "Nats";

    [Required, RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Name must be alphanumeric with optional dashes/underscores")]
    public string Name = "Endpointer_Json_Remapping";

    [Required]
    [RegularExpression(@"^(nats|tls)://[a-zA-Z0-9\.\-_]+(:\d+)?$",
        ErrorMessage = "Must be a valid NATS URL (e.g., nats://nats:4222 or tls://server.com)")]
    public required string Url { get; init; }
    [Required, RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Stream name must be alphanumeric with optional dashes/underscores")]
    public string JsonMappingStreamName { get; init; } = "json_mapping";

    [Range(1, 250, ErrorMessage = "MaxConcurrentMessages must be between 1 and 250")]
    public int MaxConcurrentMessages { get; init; } = 100;
}