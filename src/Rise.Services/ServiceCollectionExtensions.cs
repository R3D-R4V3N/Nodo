using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rise.Persistence;
using Rise.Services.Chats;
using Rise.Services.Images;
using Rise.Services.UserConnections;
using Rise.Shared.Chats;
using Rise.Shared.UserConnections;

namespace Rise.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IUserConnectionService, UserConnectionService>();
        services.AddTransient<DbSeeder>();

        services.Configure<VisionModerationOptions>(configuration.GetSection("GoogleVision:SafeSearch"));

        services.AddSingleton(sp =>
        {
            var credentialJson = configuration["GoogleVision:CredentialJson"];
            var credentialPath = configuration["GoogleVision:CredentialPath"];

            var builder = new ImageAnnotatorClientBuilder();

            if (!string.IsNullOrWhiteSpace(credentialJson))
            {
                builder.Credential = GoogleCredential.FromJson(credentialJson);
            }
            else if (!string.IsNullOrWhiteSpace(credentialPath))
            {
                builder.Credential = GoogleCredential.FromFile(credentialPath);
            }

            return builder.Build();
        });

        services.AddScoped<IImageModerationService, GoogleVisionImageModerationService>();

        // Add other application services here.
        return services;
    }
}
