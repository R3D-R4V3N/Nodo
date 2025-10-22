using System.Text.Json.Serialization;

namespace Rise.Client.Chats.Components;

public sealed record RecordedAudio
{
    [JsonPropertyName("dataUrl")]
    public string DataUrl { get; init; } = string.Empty;

    [JsonPropertyName("durationSeconds")]
    public double DurationSeconds { get; init; }
}
