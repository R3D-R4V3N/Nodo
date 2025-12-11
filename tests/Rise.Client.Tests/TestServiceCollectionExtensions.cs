using Blazored.Toast.Services;
using BlazorSpinner;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Rise.Client.Chats;
using Rise.Client.State;

namespace Microsoft.Extensions.DependencyInjection;

public static class TestServiceCollectionExtensions
{
    public static IServiceCollection AddClientTestDefaults(this IServiceCollection services)
    {
        services.TryAddSingleton<IToastService>(_ => Substitute.For<IToastService>());
        services.TryAddSingleton<SpinnerService>();
        services.TryAddSingleton<ChatState>();
        services.TryAddSingleton<IVoiceRecorderService>(_ => Substitute.For<IVoiceRecorderService>());
        return services;
    }
}
