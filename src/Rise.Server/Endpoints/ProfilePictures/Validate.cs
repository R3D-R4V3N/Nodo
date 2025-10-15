using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using Rise.Shared.ProfilePictures;

namespace Rise.Server.Endpoints.ProfilePictures;

public class Validate(IHttpClientFactory httpClientFactory, IOptions<GoogleVisionOptions> options, ILogger<Validate> logger)
    : Endpoint<ProfilePictureValidationRequest, ProfilePictureValidationResponse>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly GoogleVisionOptions _options = options.Value;
    private readonly ILogger<Validate> _logger = logger;

    public override void Configure()
    {
        Post("/api/profile-pictures/validate");
        AllowAnonymous();
    }

    public override async Task<ProfilePictureValidationResponse> ExecuteAsync(ProfilePictureValidationRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.ImageBase64))
        {
            return new ProfilePictureValidationResponse
            {
                IsApproved = false,
                Message = "Er is geen afbeelding ontvangen voor de validatie."
            };
        }

        if (string.IsNullOrWhiteSpace(_options.ServiceAccountJson))
        {
            _logger.LogError("Google Vision service account JSON is not configured. Please set GoogleVision:ServiceAccountJson in configuration.");
            return new ProfilePictureValidationResponse
            {
                IsApproved = false,
                Message = "Validatie is momenteel niet beschikbaar. Neem contact op met de beheerder."
            };
        }

        try
        {
            GoogleCredential credential;
            try
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(_options.ServiceAccountJson));
                credential = GoogleCredential.FromStream(stream).CreateScoped("https://www.googleapis.com/auth/cloud-platform");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google Vision service account configuratie is ongeldig.");
                return new ProfilePictureValidationResponse
                {
                    IsApproved = false,
                    Message = "De dienst is niet juist geconfigureerd. Neem contact op met de beheerder.",
                };
            }

            string accessToken;
            try
            {
                accessToken = await credential.GetAccessTokenForRequestAsync("https://vision.googleapis.com/", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kon geen toegangstoken ophalen voor de Google Vision API.");
                return new ProfilePictureValidationResponse
                {
                    IsApproved = false,
                    Message = "Authenticatie bij de afbeeldingscontrole is mislukt. Probeer het later opnieuw.",
                };
            }

            var httpClient = _httpClientFactory.CreateClient(nameof(Validate));
            var requestPayload = new GoogleVisionAnnotateRequest(req.ImageBase64);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://vision.googleapis.com/v1/images:annotate")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestPayload, SerializerOptions), Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await httpClient.SendAsync(httpRequest, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google Vision API returned status {StatusCode}", response.StatusCode);
                return new ProfilePictureValidationResponse
                {
                    IsApproved = false,
                    Message = "De afbeeldingscontrole kon niet worden voltooid. Probeer het later opnieuw."
                };
            }

            var visionResponse = await response.Content.ReadFromJsonAsync<GoogleVisionAnnotateResponse>(SerializerOptions, ct);
            if (visionResponse?.Responses is null || visionResponse.Responses.Count == 0)
            {
                _logger.LogWarning("Google Vision API did not return any responses.");
                return new ProfilePictureValidationResponse
                {
                    IsApproved = false,
                    Message = "Er kwam geen resultaat terug van de afbeeldingscontrole."
                };
            }

            var safeSearch = visionResponse.Responses[0].SafeSearchAnnotation;
            if (safeSearch is null)
            {
                return new ProfilePictureValidationResponse
                {
                    IsApproved = false,
                    Message = "De afbeelding kon niet worden beoordeeld. Probeer een andere afbeelding."
                };
            }

            var issues = new List<string>();
            if (IsLikelyOrVeryLikely(safeSearch.Adult))
            {
                issues.Add("naaktheid");
            }

            if (IsLikelyOrVeryLikely(safeSearch.Racy))
            {
                issues.Add("ongepaste badkleding of seksuele inhoud");
            }

            if (IsLikelyOrVeryLikely(safeSearch.Violence))
            {
                issues.Add("gewelddadige inhoud");
            }

            if (IsLikelyOrVeryLikely(safeSearch.Medical))
            {
                issues.Add("medische beelden");
            }

            if (issues.Count > 0)
            {
                var reason = $"De afbeelding lijkt {string.Join(" en ", issues)} te bevatten volgens Google Vision. Kies een andere foto.";
                return new ProfilePictureValidationResponse
                {
                    IsApproved = false,
                    Message = reason
                };
            }

            return new ProfilePictureValidationResponse
            {
                IsApproved = true,
                Message = "Google Vision markeerde deze afbeelding als veilig."
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while validating profile picture");
            return new ProfilePictureValidationResponse
            {
                IsApproved = false,
                Message = "Er trad een fout op tijdens de validatie. Probeer het later opnieuw."
            };
        }
    }

    private static bool IsLikelyOrVeryLikely(string? category) =>
        string.Equals(category, "LIKELY", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(category, "VERY_LIKELY", StringComparison.OrdinalIgnoreCase);

    private sealed record GoogleVisionAnnotateRequest
    {
        [JsonPropertyName("requests")]
        public List<GoogleVisionAnnotateItem> Requests { get; } = new()
        {
            new GoogleVisionAnnotateItem()
        };

        public GoogleVisionAnnotateRequest(string imageBase64)
        {
            Requests[0].Image.Content = imageBase64;
        }
    }

    private sealed record GoogleVisionAnnotateItem
    {
        [JsonPropertyName("image")]
        public GoogleVisionImage Image { get; } = new();

        [JsonPropertyName("features")]
        public List<GoogleVisionFeature> Features { get; } = new()
        {
            new GoogleVisionFeature { Type = "SAFE_SEARCH_DETECTION" }
        };
    }

    private sealed record GoogleVisionImage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private sealed record GoogleVisionFeature
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    private sealed record GoogleVisionAnnotateResponse
    {
        [JsonPropertyName("responses")]
        public List<GoogleVisionAnnotateResult> Responses { get; init; } = new();
    }

    private sealed record GoogleVisionAnnotateResult
    {
        [JsonPropertyName("safeSearchAnnotation")]
        public GoogleVisionSafeSearchAnnotation? SafeSearchAnnotation { get; init; }
    }

    private sealed record GoogleVisionSafeSearchAnnotation
    {
        [JsonPropertyName("adult")]
        public string? Adult { get; init; }

        [JsonPropertyName("violence")]
        public string? Violence { get; init; }

        [JsonPropertyName("racy")]
        public string? Racy { get; init; }

        [JsonPropertyName("medical")]
        public string? Medical { get; init; }
    }
}

public class GoogleVisionOptions
{
    public string? ServiceAccountJson { get; set; }
}
