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

    builder.Services.AddDbContext<ApplicationDbContext>(o =>
    {
        var cs =
            "Server=65.109.132.74;Port=3308;Database=nododb;User=chatuser;Password=chatuserpassword123;SslMode=None;";
        cs ??= builder.Configuration.GetConnectionString("DatabaseConnection")
                 ?? throw new InvalidOperationException("No connection string found");

        o.UseMySql(cs, ServerVersion.AutoDetect(cs));
        o.EnableDetailedErrors();
        if (builder.Environment.IsDevelopment())
            o.EnableSensitiveDataLogging();

        o.UseTriggers(options => options.AddTrigger<EntityBeforeSaveTrigger>());
    });

    builder.Services
        .AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services
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

        //db.Database.EnsureDeleted();
        db.Database.Migrate();

        await seeder.SeedAsync();
    //}

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
       })
       .UseSwaggerGen();

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