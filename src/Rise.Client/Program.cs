using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Rise.Client;
using Rise.Client.Chats;
using Rise.Client.Identity;
using Rise.Shared.Chats;

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
    //  Woordfilter hier
    builder.Services.AddSingleton<Rise.Services.WoordFilter>();

    // configure client for auth interactions
    builder.Services.AddHttpClient("SecureApi",opt => opt.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001"))
        .AddHttpMessageHandler<CookieHandler>();

    builder.Services.AddHttpClient<IChatService, ChatService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001");
    }).AddHttpMessageHandler<CookieHandler>();

    await builder.Build().RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An exception occurred while creating the WASM host");
    throw;
}
