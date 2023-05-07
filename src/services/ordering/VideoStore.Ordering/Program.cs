using Serilog;
using System.IdentityModel.Tokens.Jwt;
using VideoStore.Movies.Infrastrucutre.Repositories;
using VideoStore.Ordering.Constants;
using VideoStore.Ordering.Extensions;
using VideoStore.Ordering.Handlers;
using VideoStore.Ordering.Models;
using VideoStore.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); // remove default logging providers
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // clear Microsoft changed claim names from dictionary and preserve original ones

try
{
    builder.Logging.AddSerilog(LoggingConfiguration.CreateLogger(builder.Environment));
    var configuration = GetConfiguration(builder.Environment);

    Log.Information("Configuring web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    builder.Configuration.ConfigureAzureKeyVault();
    builder.Services.AddAzureClients(builder.Configuration);
    builder.Services.ConfigureDbContext(builder.Configuration);
    builder.Services.AddRedisCaching(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.ConfigureSwagger();
    builder.Services.AddTransient<IOrderingRepository, OrderingRepository>();
    builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(OrderingConstants.JwtConfigurationName));
    builder.Services.AddTransient(typeof(IIdempotentMessageHandler<>), typeof(IdempotentMessageHandlerDecorator<>));
    builder.Services.ConfigureAuthentication(builder.Configuration);
    builder.Services.ConfigureServiceBus(builder.Configuration);

    Log.Information("Starting web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/V1/swagger.json", "Ordering WebAPI");
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
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", builder.Environment.ApplicationName);
}
finally
{
    Log.CloseAndFlush();
}

IConfiguration GetConfiguration(IWebHostEnvironment environment)
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    //var config = builder.Build();

    //if (config.GetValue<bool>("UseVault", false))
    //{
    //    TokenCredential credential = new ClientSecretCredential(
    //        config["Vault:TenantId"],
    //        config["Vault:ClientId"],
    //        config["Vault:ClientSecret"]);
    //    builder.AddAzureKeyVault(new Uri($"https://{config["Vault:Name"]}.vault.azure.net/"), credential);
    //}

    return builder.Build();
}