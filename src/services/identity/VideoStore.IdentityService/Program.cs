using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using VideoStore.IdentityService.Infrastrucutre;
using VideoStore.IdentityService.Infrastrucutre.Repositories;
using VideoStore.IdentityService.Model;
using VideoStore.IdentityService.Services;
using VideoStore.Shared;

const string JwtConfigurationName = "JWT";
const string IdentityConnectionStringKey = "IdentityConnectionString";
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); // remove default logging providers
try
{
    builder.Logging.AddSerilog(LoggingConfiguration.CreateLogger(builder.Environment));
    var configuration = GetConfiguration(builder.Environment);

    Log.Information("Configuring web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    ConfigureDbContext(builder, configuration);
    builder.Services.AddTransient<IUserRepository, UserRepository>();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(JwtConfigurationName));
    builder.Services.AddScoped<TokenService>();

    Log.Information("Starting web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

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

void ConfigureDbContext(WebApplicationBuilder builder, IConfiguration configuration)
{
    builder.Services.AddDbContext<IdentityContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString(IdentityConnectionStringKey), option =>
                    {
                        option.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
                        // EF connection resiliency
                        option.EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    }));

    builder.Host.MigrateDatabase<IdentityContext>(builder.Services, (context, services) =>
    {
        var logger = services.GetService<ILogger<IdentityContextSeed>>();
        IdentityContextSeed.SeedAsync(context, logger).Wait();
    });
}
