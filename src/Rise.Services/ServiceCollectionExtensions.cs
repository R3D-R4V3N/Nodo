using Microsoft.Extensions.DependencyInjection;
using Rise.Persistence;
using Rise.Services.Chats;
using Rise.Shared.Chats;
using Rise.Services.Events;
using Rise.Services.Organizations;
using Rise.Services.RegistrationRequests;
using Rise.Services.UserConnections;
using Rise.Services.Users;
using Rise.Shared.Events;
using Rise.Shared.Organizations;
using Rise.Shared.RegistrationRequests;
using Rise.Shared.UserConnections;
using Rise.Shared.Users;
using Rise.Shared.UserSentiments;
using Rise.Services.Sentiments;
using Rise.Services.Hobbies;
using Rise.Shared.Hobbies;
using Rise.Services.Validators;
using Rise.Shared.Validators;

namespace Rise.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserConnectionService, UserConnectionService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IRegistrationRequestService, RegistrationRequestService>();
        services.AddScoped<ISentimentsService, SentimentService>();
        services.AddScoped<IHobbyService, HobbyService>();
        services.AddScoped<IValidatorService, ValidatorService>();
        services.AddTransient<DbSeeder>();
        
        // Add other application services here.
        return services;
    }
}
