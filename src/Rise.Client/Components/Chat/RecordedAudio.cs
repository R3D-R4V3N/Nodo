using System.Text.Json.Serialization;

namespace Rise.Client.Components.Chat;

public sealed record RecordedAudio
{
    [JsonPropertyName("dataUrl")]
    public string DataUrl { get; init; } = string.Empty;

    [JsonPropertyName("durationSeconds")]
    public double DurationSeconds { get; init; }
}
