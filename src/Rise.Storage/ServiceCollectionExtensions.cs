using Microsoft.Extensions.DependencyInjection;
using Rise.Services.BlobStorage;

namespace Rise.Storage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlobStorageServices(this IServiceCollection services)
    {
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        return services;
    }
}
