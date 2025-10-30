using Rise.Domain.Messages;

namespace Rise.Services;

internal class AudioHelperMethods
{
    public static string? BuildAudioDataUrl(Message message)
    {
        if (message.AudioData is not { Length: > 0 } || string.IsNullOrWhiteSpace(message.AudioContentType))
        {
            return null;
        }

        var base64 = Convert.ToBase64String(message.AudioData);
        return $"data:{message.AudioContentType};base64,{base64}";
    }

    public static bool TryParseAudioDataUrl(
        string audioDataUrl,
        out string? contentType,
        out byte[]? data,
        out string? errorMessage)
    {
        contentType = null;
        data = null;
        errorMessage = null;

        if (!audioDataUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "Audio data-URL moet starten met 'data:'.";
            return false;
        }

        var commaIndex = audioDataUrl.IndexOf(',');
        if (commaIndex <= 0 || commaIndex >= audioDataUrl.Length - 1)
        {
            errorMessage = "Audio data-URL mist inhoud.";
            return false;
        }

        var metadata = audioDataUrl.Substring("data:".Length, commaIndex - "data:".Length);
        if (string.IsNullOrWhiteSpace(metadata))
        {
            errorMessage = "Audio contenttype ontbreekt.";
            return false;
        }

        var base64MarkerIndex = metadata.IndexOf(";base64", StringComparison.OrdinalIgnoreCase);
        if (base64MarkerIndex < 0)
        {
            errorMessage = "Audio data-URL moet base64-gecodeerd zijn.";
            return false;
        }

        var contentTypeSegment = metadata[..base64MarkerIndex];
        var contentTypeParts = contentTypeSegment
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        contentType = contentTypeParts.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(contentType))
        {
            errorMessage = "Audio contenttype ontbreekt.";
            return false;
        }

        var base64 = audioDataUrl[(commaIndex + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(base64))
        {
            errorMessage = "Audio bevat geen data.";
            return false;
        }

        try
        {
            data = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            errorMessage = "Audio kon niet gedecodeerd worden.";
            return false;
        }

        if (data.Length == 0)
        {
            errorMessage = "Audio bevat geen data.";
            return false;
        }

        return true;
    }
}
