using Destructurama;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Persistence;
using Rise.Persistence.Triggers;
using Rise.Server.Identity;
using Rise.Server.Processors;
using Rise.Server.RealTime;
using Rise.Services;
using Rise.Services.Chats;
using Rise.Services.Identity;
using Rise.Services.UserConnections;
using Rise.Shared.Chats;
using Serilog.Events;
using Rise.Server.Hubs;
using Rise.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Rise.Server.Push;
using Rise.Services.Notifications;
using Azure.Storage.Blobs;
using Serilog; // Zorg dat deze using er is voor Log
using Microsoft.OpenApi.Models;
using System.Reflection;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);

    // Trust X-Forwarded headers from NGINX
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
            Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    });

    // Serilog uit config
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Destructure.UsingAttributes());

    builder.Services.AddSingleton(x =>
    {
        var blobConnectionstring = Environment.GetEnvironmentVariable("BLOB_CONNECTION")
                     ?? builder.Configuration.GetConnectionString("BlobConnection")
                     ?? throw new InvalidOperationException("No blob connection string found");
        blobConnectionstring = blobConnectionstring.Trim();

        return new BlobServiceClient(blobConnectionstring);
    });

    builder.Services.AddDbContext<ApplicationDbContext>(o =>
    {
        var dbConnectionstring = Environment.GetEnvironmentVariable("DB_CONNECTION")
                         ?? builder.Configuration.GetConnectionString("DatabaseConnection")
                         ?? throw new InvalidOperationException("No db connection string found");
        dbConnectionstring = dbConnectionstring.Trim();

        o.UseMySql(dbConnectionstring, ServerVersion.AutoDetect(dbConnectionstring));
        o.EnableDetailedErrors();
        if (builder.Environment.IsDevelopment())
            o.EnableSensitiveDataLogging();

        o.UseTriggers(options => options.AddTrigger<EntityBeforeSaveTrigger>());
    });

    builder.Services
        .AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
    
    builder.Services.Configure<VapidOptions>(builder.Configuration.GetSection("Vapid"));

    builder.Services.AddSingleton<IPushSubscriptionStore, InMemoryPushSubscriptionStore>();
    builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();

    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RISE API",
                Version = "v1"
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        })
        .AddHttpContextAccessor()
        .AddScoped<ISessionContextProvider, HttpContextSessionProvider>()
        .AddApplicationServices()
        .AddBlobStorageServices()
        .AddAuthorization()
        .AddFastEndpoints(opt =>
        {
            opt.IncludeAbstractValidators = true;
            opt.Assemblies = [typeof(ChatRequest.CreateMessage).Assembly];
        })
        .SwaggerDocument(o =>
        {
            o.DocumentSettings = s => { s.Title = "RISE API"; };
        });

    builder.Services.AddSignalR();
    builder.Services.AddSingleton<IChatMessageDispatcher, SignalRChatMessageDispatcher>();
    builder.Services.AddSingleton<IUserConnectionNotificationDispatcher, SignalRUserConnectionNotificationDispatcher>();

    var app = builder.Build();
    
    //if (app.Environment.IsDevelopment())
    //{
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();

        db.Database.EnsureDeleted();
        db.Database.Migrate();

        await seeder.SeedAsync();
    //}

    // De rest van de middleware pipeline
    app.UseHttpsRedirection()
        .UseBlazorFrameworkFiles()
        .UseStaticFiles()
        .UseDefaultExceptionHandler()
        .UseAuthentication()
        .UseAuthorization()
        .UseFastEndpoints(o =>
        {
            o.Endpoints.Configurator = ep =>
            {
                ep.DontAutoSendResponse();
                ep.PreProcessor<GlobalRequestLogger>(Order.Before);
                ep.PostProcessor<GlobalResponseSender>(Order.Before);
                ep.PostProcessor<GlobalResponseLogger>(Order.Before);
            };
        });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        });
    }

    app.MapHub<Chathub>("/chathub");
    app.MapHub<UserConnectionHub>("/connectionsHub");

    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
}
finally
{
    Log.CloseAndFlush();
}