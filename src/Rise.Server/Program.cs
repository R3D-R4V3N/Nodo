using Destructurama;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Rise.Persistence;
using Rise.Persistence.Triggers;
using Rise.Server.Identity;
using Rise.Server.Processors;
using Rise.Services;
using Rise.Services.Identity;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);

    // Serilog uit config
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Destructure.UsingAttributes());

    // DbContext + Identity
    builder.Services
        .AddDbContext<ApplicationDbContext>(o =>
        {
            var cs = Environment.GetEnvironmentVariable("DB_CONNECTION");
            cs ??= builder.Configuration.GetConnectionString("DatabaseConnection")
                     ?? throw new InvalidOperationException("Connection string 'DatabaseConnection' not found.");
            var configuredVersion = Environment.GetEnvironmentVariable("DB_SERVER_VERSION")
                                   ?? builder.Configuration["Database:ServerVersion"];

            var serverVersion = MySqlServerVersionResolver.Resolve(cs, configuredVersion);

            o.UseMySql(cs, serverVersion);
            o.EnableDetailedErrors();
            if (builder.Environment.IsDevelopment())
                o.EnableSensitiveDataLogging();

            o.UseTriggers(options => options.AddTrigger<EntityBeforeSaveTrigger>());
        });

    builder.Services
        .AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Overige DI
    builder.Services
        .AddHttpContextAccessor()
        .AddScoped<ISessionContextProvider, HttpContextSessionProvider>()
        .AddApplicationServices()
        .AddAuthorization()
        .AddFastEndpoints(opt =>
        {
            opt.IncludeAbstractValidators = true;
            opt.Assemblies = [typeof(Rise.Shared.Products.ProductRequest).Assembly];
        })
        .SwaggerDocument(o =>
        {
            o.DocumentSettings = s => { s.Title = "RISE API"; };
        });

    // Seeder registreren (als je een DbSeeder hebt)
    builder.Services.AddScoped<DbSeeder>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();

        //db.Database.EnsureDeleted(); // Delete the database if it exists to clean it up if needed.

        try
        {
            db.Database.Migrate();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Skipping database migration and seed because the connection could not be established.");
        }
    }

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
