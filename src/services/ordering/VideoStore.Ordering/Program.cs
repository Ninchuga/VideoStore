using Serilog;
using System.IdentityModel.Tokens.Jwt;
using VideoStore.Ordering.Constants;
using VideoStore.Ordering.Extensions;
using VideoStore.Ordering.Handlers;
using VideoStore.Ordering.Infrastrucutre.Repositories;
using VideoStore.Ordering.Models;
using VideoStore.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); // remove default logging providers
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // clear Microsoft changed claim names from dictionary and preserve original ones
var logger = LoggingConfiguration.CreateLogger(builder.Environment);
builder.Logging.AddSerilog(logger);

try
{
    var configuration = GetConfiguration(builder.Environment);

    logger.Information("Configuring web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    builder.Configuration.ConfigureAzureKeyVault();
    builder.Services.ConfigureAzureClients(builder.Configuration);
    builder.Services.ConfigureDbContext(builder.Host, builder.Configuration, builder.Environment);
    builder.Services.AddRedisCaching(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.ConfigureSwagger();
    builder.Services.AddTransient<IOrderingRepository, OrderingRepository>();
    builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(OrderingConstants.JwtConfigurationName));
    builder.Services.AddTransient(typeof(IIdempotentMessageHandler<>), typeof(IdempotentMessageHandlerDecorator<>));
    builder.Services.ConfigureAuthentication(builder.Configuration);
    builder.Services.ConfigureServiceBus(builder.Configuration);

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
    logger.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", builder.Environment.ApplicationName);
}

IConfiguration GetConfiguration(IWebHostEnvironment environment)
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    return builder.Build();
}


