using VideoStore.Movies.Infrastrucutre;
using VideoStore.Movies;
using VideoStore.Shared;
using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using VideoStore.Movies.Infrastrucutre.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VideoStore.Movies.Models;
using Microsoft.OpenApi.Models;

const string JwtConfigurationName = "JWT";
const string MoviesConnectionStringKey = "MoviesConnectionString";
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); // remove default logging providers
try
{
    builder.Logging.AddSerilog(LoggingConfiguration.CreateLogger(builder.Environment));
    var configuration = GetConfiguration(builder.Environment);

    Log.Information("Configuring web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    ConfigureDbContext(builder, configuration);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    ConfigureSwagger(builder);
    builder.Services.AddTransient<IMovieRepository, MovieRepository>();
    builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(JwtConfigurationName));
    ConfigureAuthentication(builder);

    Log.Information("Starting web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/V1/swagger.json", "Movies WebAPI");
        });
    }

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

void ConfigureDbContext(WebApplicationBuilder builder, IConfiguration configuration)
{
    builder.Services.AddDbContext<MovieContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString(MoviesConnectionStringKey), option =>
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
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    var jwtConfig = builder.Configuration.GetSection(JwtConfigurationName).Get<JwtConfig>();

    builder.Services.AddAuthentication(opt => 
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true; // stores the bearer token in HTTP Context, so we can use the token later in the controller
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = jwtConfig?.Audience,
                ValidIssuer = jwtConfig?.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig?.Secret))
            };
        });
}

void ConfigureSwagger(WebApplicationBuilder builder)
{
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("V1", new OpenApiInfo
        {
            Version = "V1",
            Title = "Movie API",
            Description = "Movies WebAPI"
        });
        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
        {
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Bearer Authentication with JWT Token",
            Type = SecuritySchemeType.Http
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                            Type = ReferenceType.SecurityScheme
                    }
                },
                new List <string>()
            }
        });
    });
}