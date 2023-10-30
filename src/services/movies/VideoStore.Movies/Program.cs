using Serilog;
using System.IdentityModel.Tokens.Jwt;
using VideoStore.Movies.Constants;
using VideoStore.Movies.Extensions;
using VideoStore.Movies.Infrastrucutre.Repositories;
using VideoStore.Movies.Models;
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

    if (!builder.Environment.IsDevelopment())
        builder.Configuration.ConfigureAzureKeyVault();

    builder.Services.ConfigureAzureClients(builder.Configuration);
    builder.Services.ConfigureDbContext(builder.Host, builder.Configuration);
    builder.Services.AddRouting(options => { options.LowercaseUrls = true; options.LowercaseQueryStrings = true; });
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.ConfigureSwagger();
    builder.Services.AddTransient<IMovieRepository, MovieRepository>();
    builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(MoviesConstants.JwtConfigurationName));
    builder.Services.ConfigureAuthentication(builder.Configuration);
    builder.Services.ConfigureServiceBus(builder.Configuration, logger);

    var app = builder.Build();

    app.Logger.LogInformation("Starting web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/V1/swagger.json", "Movies WebAPI");
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