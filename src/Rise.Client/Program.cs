using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FluentValidation;
using Rise.Client;
using Rise.Client.Chats;
using Rise.Client.Events;
using Rise.Client.Identity;
using Rise.Client.RealTime;
using Rise.Client.State;
using Rise.Client.Emergencies;
using Rise.Client.UserConnections;
using Rise.Client.Users;
using Rise.Client.Offline;
using Rise.Shared.Chats;
using Rise.Shared.Events;
using Rise.Shared.Emergencies;
using Rise.Shared.UserConnections;
using Rise.Shared.Users;
using Rise.Shared.Validators;
using UserService = Rise.Client.Users.UserService;
using Rise.Shared.Organizations;
using Rise.Client.Organizations;
using Rise.Shared.RegistrationRequests;
using Rise.Client.RegistrationRequests;



// For BACKEND_URL
using System.Net.Http.Json;
using Blazored.Toast;
using Rise.Shared.Validators;
using Rise.Client.Validators;
using Rise.Shared.Identity.Accounts;

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

    builder.Services.AddHttpClient<IEmergencyService, EmergencyService>(client =>
    {
        client.BaseAddress = backendUri;
    }).AddHttpMessageHandler<CookieHandler>();

    builder.Services.AddHttpClient<IUserService, UserService>(client =>
    {
        client.BaseAddress = backendUri;
    });

    builder.Services.AddHttpClient<UserContextService>(client =>
    {
        client.BaseAddress = backendUri;
    });

    builder.Services.AddHttpClient<IOrganizationService, OrganizationService>(client =>
    {
        client.BaseAddress = backendUri;
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

    // load in rules
    builder.Services.AddHttpClient<IValidatorService, ValidatorService>(client =>
    {
        client.BaseAddress = backendUri;
    });

    // current user
    builder.Services.AddSingleton<UserState>();

    builder.Services.AddScoped<IVoiceRecorderService, VoiceRecorderService>();
    builder.Services.AddScoped<ChatMessageDispatchService>();

    builder.Services.AddSingleton<IHubClientFactory, HubClientFactory>();
    builder.Services.AddSingleton<IHubClient, HubClient>();

    builder.Services.AddSingleton<OfflineQueueService>();
    builder.Services.AddSingleton<ConnectionServiceFactory>();

    // load in validation rules for DI
    // no clue if this is the ideal location tho
    var validatorServiceTemp = new ValidatorService(http);
    var rules = await validatorServiceTemp.GetRulesAsync();
    builder.Services.AddSingleton(rules);

    builder.Services.AddTransient<IValidator<AccountRequest.Login>>(sp =>
        new AccountRequest.Login.Validator(sp.GetRequiredService<ValidatorRules>()));

    builder.Services.AddTransient<IValidator<AccountRequest.Register>>(sp =>
        new AccountRequest.Register.Validator(sp.GetRequiredService<ValidatorRules>()));

    builder.Services.AddTransient<IValidator<UserRequest.UpdateCurrentUser>>(sp =>
        new UserRequest.UpdateCurrentUserValidator(sp.GetRequiredService<ValidatorRules>()));

    builder.Services.AddTransient<IValidator<ChatRequest.CreateMessage>>(sp =>
        new ChatRequest.CreateMessage.Validator(sp.GetRequiredService<ValidatorRules>()));

    var host = builder.Build();

    var offlineQueue = host.Services.GetRequiredService<OfflineQueueService>();
    await offlineQueue.StartAsync();

    var connectionFactory = host.Services.GetRequiredService<ConnectionServiceFactory>();
    var connectionService = await connectionFactory.CreateAsync();
    await connectionService.InitializeAsync();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An exception occurred while creating the WASM host");
    throw;
}
