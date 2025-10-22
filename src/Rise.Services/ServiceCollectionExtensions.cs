using Microsoft.Extensions.DependencyInjection;
using Rise.Persistence;
using Rise.Services.Chats;
using Rise.Shared.Chats;
using Rise.Services.UserConnections;
using Rise.Shared.UserConnections;
<<<<<<< HEAD
=======
using Rise.Shared.Users;
using Rise.Services.Users;
>>>>>>> codex/add-alert-message-for-supervisor-monitoring

namespace Rise.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();
<<<<<<< HEAD
        services.AddTransient<DbSeeder>();

=======
        services.AddScoped<IUserService, UserService>();
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
        services.AddScoped<IUserConnectionService, UserConnectionService>();
        services.AddTransient<DbSeeder>();       
        
        // Add other application services here.
        return services;
    }
}
