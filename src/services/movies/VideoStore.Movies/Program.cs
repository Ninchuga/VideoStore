using VideoStore.Movies.Infrastrucutre;
using VideoStore.Movies;
using VideoStore.Shared;
using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using VideoStore.Movies.Repositories;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders(); // remove default logging providers
    builder.Logging.AddSerilog(LoggingConfiguration.CreateLogger(builder.Environment));
    var configuration = GetConfiguration(builder.Environment);

    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    
    builder.Services.AddDbContext<MovieContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("OrderingConnectionString"), option =>
                {
                    option.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                    // EF connection resiliency
                    option.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                }));

    builder.Host.MigrateDatabase<MovieContext>(builder.Services, (context, services) =>
    {
        var logger = services.GetService<ILogger<MovieContextSeed>>();
        MovieContextSeed.SeedAsync(context, logger).Wait();
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddTransient<IMovieRepository, MovieRepository>();

    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
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

public partial class Program
{
    public static string Namespace = typeof(Startup).Namespace;
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
}

