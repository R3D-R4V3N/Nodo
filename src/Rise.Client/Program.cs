using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Rise.Client;
using Rise.Client.Chats;
using Rise.Client.Identity;
<<<<<<< HEAD
using Rise.Shared.Chats;
using Rise.Client.UserConnections;
using Rise.Shared.UserConnections;
=======
using Rise.Client.UserConnections;
using Rise.Client.Users;
using Rise.Services.Users;
using Rise.Shared.Chats;
using Rise.Shared.UserConnections;
using Rise.Shared.Users;
>>>>>>> codex/add-alert-message-for-supervisor-monitoring

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
<<<<<<< HEAD
    //  Woordfilter hier
    builder.Services.AddSingleton<Rise.Services.WordFilter>();
=======
>>>>>>> codex/add-alert-message-for-supervisor-monitoring

    var backendUri = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001");

<<<<<<< HEAD
=======

    //  word filter
    builder.Services.AddSingleton<Rise.Services.WordFilter>();

    var backendUri = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001");

>>>>>>> codex/add-alert-message-for-supervisor-monitoring
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

<<<<<<< HEAD
=======
    builder.Services.AddHttpClient<IUserService, UserService>(client =>
    {
        client.BaseAddress = backendUri;
    });

    // current user
    builder.Services.AddHttpClient<UserContextService>(client =>
    {
        client.BaseAddress = backendUri;
    });

>>>>>>> codex/add-alert-message-for-supervisor-monitoring
    await builder.Build().RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An exception occurred while creating the WASM host");
    throw;
}
