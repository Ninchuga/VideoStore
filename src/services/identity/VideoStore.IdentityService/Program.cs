using Serilog;
using VideoStore.IdentityService.Constants;
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

    builder.Services.ConfigureDbContext(builder.Host, configuration);
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
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionHandlerMiddleware>();

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
