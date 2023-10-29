using Serilog;
using VideoStore.IdentityService.Constants;
using VideoStore.IdentityService.Extensions;
using VideoStore.IdentityService.Infrastrucutre.Repositories;
using VideoStore.IdentityService.Model;
using VideoStore.IdentityService.Services;
using VideoStore.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); // remove default logging providers
var logger = LoggingConfiguration.CreateLogger(builder.Environment);
builder.Logging.AddSerilog(logger);

try
{
    builder.Logging.AddAzureWebAppDiagnostics(); // provides logger implementations to support Azure App Service diagnostics logs and log streaming features.
    var configuration = GetConfiguration(builder.Environment);

    logger.Information("Configuring web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    if(!builder.Environment.IsDevelopment())
        builder.Configuration.ConfigureAzureKeyVault();

    builder.Services.ConfigureAzureClients(builder.Configuration);
    builder.Services.ConfigureDbContext(builder.Host, builder.Configuration);
    builder.Services.ConfigureAuthentication(builder.Configuration);
    builder.Services.AddTransient<IUserRepository, UserRepository>();

    // 1. If you're using WebApp service use Microsoft.ApplicationInsights.AspNetCore nuget package,
    // and specify IConfiguration argument if there is another configuration source than just appsettings.json
    // 2. If your app is running as Docker container on Kubernetes, use Microsoft.ApplicationInsights.Kubernetes nuget package
    // 3. If your app is running in Service Fabric, use Microsoft.ApplicationInsights.ServiceFabric nuget package
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = builder.Configuration[IdentityConstants.IdentityAppInsightsConnectionStringKey];
    });

    // Add this line of code to enable Profiler from nuget -> Microsoft.ApplicationInsights.Profiler.AspNetCore
    //builder.Services.AddServiceProfiler(); 
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.ConfigureSwagger();
    builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(IdentityConstants.JwtConfigurationName));
    builder.Services.AddScoped<TokenService>();

    var app = builder.Build();

    app.Logger.LogInformation("Starting web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/V1/swagger.json", "Identity Service WebAPI");
        });
    }

    app.UseMiddleware<ExceptionHandlerMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", builder.Environment.ApplicationName);
}

IConfiguration GetConfiguration(IHostEnvironment environment)
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    return builder.Build();
}