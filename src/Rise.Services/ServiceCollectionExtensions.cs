using Microsoft.Extensions.DependencyInjection;
using Rise.Persistence;
using Rise.Services.Chats;
using Rise.Shared.Chats;
using Rise.Services.UserConnections;
using Rise.Shared.UserConnections;

namespace Rise.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();
        services.AddTransient<DbSeeder>();

        services.AddScoped<IUserConnectionService, UserConnectionService>();
        services.AddTransient<DbSeeder>();       
        
        // Add other application services here.
        return services;
    }
}
