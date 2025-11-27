using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Rise.Client;
using Rise.Client.Chats;
using Rise.Client.Events;
using Rise.Client.Identity;
using Rise.Client.RealTime;
using Rise.Client.State;
using Rise.Client.UserConnections;
using Rise.Client.Users;
using Rise.Client.Offline;
using Rise.Shared.Chats;
using Rise.Shared.Events;
using Rise.Shared.UserConnections;
using Rise.Shared.Users;
using UserService = Rise.Client.Users.UserService;
using Rise.Shared.Organizations;
using Rise.Client.Organizations;
using Rise.Shared.RegistrationRequests;
using Rise.Client.RegistrationRequests;



// For BACKEND_URL
using System.Net.Http.Json;
using Blazored.Toast;

try
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);

    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.BrowserConsole(outputTemplate:"[{Timestamp:HH:mm:ss}{Level:u3}]{Message:lj} {NewLine}{Exception}")
        .CreateLogger();

    Log.Information("Starting web application");

    // register the cookie handler
    builder.Services.AddTransient<CookieHandler>();

    // set up authorization
    builder.Services.AddAuthorizationCore();

    // register the custom state provider
    builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
    // register the account management interface
    builder.Services.AddScoped(sp => (IAccountManager)sp.GetRequiredService<AuthenticationStateProvider>());
    
    builder.Services.AddBlazoredToast();

    // Laadt config.json uit wwwroot om de backend URL dynamisch te halen; gebruikt fallback naar localhost als key ontbreekt.
    using var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    var config = await http.GetFromJsonAsync<Dictionary<string, string>>("config.json");

    var backendUrl = config?["backendUrl"] ?? "https://localhost:5001";
    var backendUri = new Uri(backendUrl);

    // configure client for auth interactions
    builder.Services.AddHttpClient("SecureApi",opt => opt.BaseAddress = backendUri)
        .AddHttpMessageHandler<CookieHandler>();

    builder.Services.AddHttpClient<IChatService, ChatService>(client =>
    {
        client.BaseAddress = backendUri;
    }).AddHttpMessageHandler<CookieHandler>();

    builder.Services.AddHttpClient<IUserConnectionService, UserConnectionService>(client =>
    {
        client.BaseAddress = backendUri;
    });

    builder.Services.AddHttpClient<IUserContextService, UserContextService>(client =>
    {
        client.BaseAddress = backendUri;
    });

    builder.Services.AddHttpClient<IUserService, UserService>(client =>
    {
        client.BaseAddress = backendUri;
    });

    builder.Services.AddHttpClient<UserContextService>(client =>
    {
        client.BaseAddress = backendUri;
    });
    // Publieke API client (geen CookieHandler)
    builder.Services.AddHttpClient<IOrganizationService, OrganizationService>(client =>
    {
        client.BaseAddress = backendUri; // bij jou bv. https://localhost:5001
    });

    builder.Services.AddHttpClient<IEventService, EventService>(client =>
    {
        client.BaseAddress = backendUri;
    }).AddHttpMessageHandler<CookieHandler>();

    builder.Services.AddHttpClient<IRegistrationRequestService, RegistrationRequestService>(client =>
        {
            client.BaseAddress = backendUri;
        })
        .AddHttpMessageHandler<CookieHandler>();

    // current user
    builder.Services.AddSingleton<UserState>();

    builder.Services.AddSingleton<IHubClientFactory, HubClientFactory>();
    builder.Services.AddSingleton<IHubClient, HubClient>();

    builder.Services.AddSingleton<OfflineQueueService>();

    var host = builder.Build();

    var offlineQueue = host.Services.GetRequiredService<OfflineQueueService>();
    await offlineQueue.StartAsync();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An exception occurred while creating the WASM host");
    throw;
}
