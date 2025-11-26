using Microsoft.Extensions.DependencyInjection;
using Rise.Services.FileServer;
using Rise.Storage.Images;
using Rise.Storage.Messages;

namespace Rise.Storage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlobStorageServices(this IServiceCollection services)
    {
        services.AddScoped<IImageStorageService, ImageStorageService>();
        services.AddScoped<IMessageStorageService, MessageStorageService>();
        return services;
    }
}
