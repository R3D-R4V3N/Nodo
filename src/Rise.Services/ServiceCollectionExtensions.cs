using Microsoft.Extensions.DependencyInjection;
using Rise.Persistence;
using Rise.Services.Chats;
using Rise.Shared.Chats;
using Rise.Services.Registrations;
using Rise.Services.UserConnections;
using Rise.Shared.Registrations;
using Rise.Shared.UserConnections;
using Rise.Shared.Users;
using Rise.Services.Users;

namespace Rise.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserConnectionService, UserConnectionService>();
        services.AddScoped<IRegistrationRequestService, RegistrationRequestService>();
        services.AddTransient<DbSeeder>();       
        
        // Add other application services here.
        return services;
    }
}
