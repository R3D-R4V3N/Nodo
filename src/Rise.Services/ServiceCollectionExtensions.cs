using Microsoft.Extensions.DependencyInjection;
using Rise.Persistence;
using Rise.Services.Chats;
using Rise.Services.Organizations;
using Rise.Shared.Chats;
using Rise.Shared.Organizations;

namespace Rise.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddTransient<DbSeeder>();

        // Add other application services here.
        return services;
    }
}
