using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bixa.Backend.Models.WebhookModel;

public class WebhookPayload<T> where T : class
{
    [JsonPropertyName("entityType")]
    public string EntityType { get; set; } = string.Empty;

    [JsonPropertyName("entityId")]
    public int EntityId { get; set; }

    [JsonPropertyName("changeType")]
    public ChangeEventType ChangeType { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("payload")]
    public T Payload { get; set; } = null!;

    [JsonPropertyName("relatedEntities")]
    public List<RelatedEntityInfo>? RelatedEntities { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public enum ChangeEventType
{
    Created,
    Updated,
    Deleted
}

public class RelatedEntityInfo
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
