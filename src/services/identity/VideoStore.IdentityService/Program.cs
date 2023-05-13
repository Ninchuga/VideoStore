using Serilog;
using VideoStore.IdentityService.Constants;
using VideoStore.IdentityService.Extensions;
using VideoStore.IdentityService.Infrastrucutre.Repositories;
using VideoStore.IdentityService.Model;
using VideoStore.IdentityService.Services;
using VideoStore.Movies.Extensions;
using VideoStore.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); // remove default logging providers
try
{
    builder.Logging.AddSerilog(LoggingConfiguration.CreateLogger(builder.Environment));
    var configuration = GetConfiguration(builder.Environment);

    Log.Information("Configuring web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    builder.Configuration.ConfigureAzureKeyVault();
    builder.Services.ConfigureAzureClients(builder.Configuration);
    builder.Services.ConfigureDbContext(builder.Host, builder.Configuration);
    builder.Services.ConfigureAuthentication(builder.Configuration);
    builder.Services.AddTransient<IUserRepository, UserRepository>();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.ConfigureSwagger();
    builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(IdentityConstants.JwtConfigurationName));
    builder.Services.AddScoped<TokenService>();

    Log.Information("Starting web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    var app = builder.Build();

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

    return builder.Build();
}
