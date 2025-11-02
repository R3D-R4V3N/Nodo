using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Rise.Client;
using Rise.Client.Chats;
using Rise.Client.Identity;
using Rise.Client.Organizations;
using Rise.Client.Registrations;
using Rise.Client.State;
using Rise.Client.UserConnections;
using Rise.Client.Users;
using Rise.Shared.Chats;
using Rise.Shared.UserConnections;
using Rise.Shared.Users;
using UserService = Rise.Client.Users.UserService;

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

    var backendUri = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001");

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

    builder.Services.AddHttpClient<IOrganizationService, OrganizationService>(client =>
    {
        client.BaseAddress = backendUri;
    });

    builder.Services.AddHttpClient<IRegistrationService, RegistrationService>(client =>
    {
        client.BaseAddress = backendUri;
    }).AddHttpMessageHandler<CookieHandler>();

    builder.Services.AddHttpClient<UserContextService>(client =>
    {
        client.BaseAddress = backendUri;
    });

    // current user
    builder.Services.AddSingleton<UserState>();

    await builder.Build().RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An exception occurred while creating the WASM host");
    throw;
}
