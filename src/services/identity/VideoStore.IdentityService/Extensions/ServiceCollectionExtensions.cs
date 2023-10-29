using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using VideoStore.IdentityService.Constants;
using VideoStore.IdentityService.Infrastrucutre;
using VideoStore.IdentityService.Model;
using VideoStore.IdentityService.Models;
using VideoStore.Shared;

namespace VideoStore.IdentityService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static readonly InMemoryDatabaseRoot InMemoryDatabaseRoot = new InMemoryDatabaseRoot();

        public static void ConfigureDbContext(this IServiceCollection services, ConfigureHostBuilder host, IConfiguration configuration)
        {
            bool useInMemoryDb = configuration.GetValue<bool>(IdentityConstants.FeatureFlags.UseInMemoryDatabase);
            if (useInMemoryDb)
            {
                services.AddDbContext<IdentityContext>(options =>
                    options.UseInMemoryDatabase("IdentityDb"));
                var serviceProvider = services.BuildServiceProvider();
                var context = serviceProvider.GetService<IdentityContext>();
                var logger = serviceProvider.GetService<ILogger<IdentityContextSeed>>();
                IdentityContextSeed.SeedAsync(context, logger).Wait();
            }
            else
            {
                services.AddDbContext<IdentityContext>(options =>
                    options.UseSqlServer(configuration[IdentityConstants.IdentityConnectionStringKey], option =>
                    {
                        option.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
                        // EF connection resiliency
                        option.EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    }));

                host.MigrateDatabase<IdentityContext>(services, (context, services) =>
                {
                    var logger = services.GetService<ILogger<IdentityContextSeed>>();
                    IdentityContextSeed.SeedAsync(context, logger).Wait();
                });
            }
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("V1", new OpenApiInfo
                {
                    Version = "V1",
                    Title = "Identity API",
                    Description = "Video store identity WebAPI"
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

        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtConfig = configuration.GetSection(IdentityConstants.JwtConfigurationName).Get<JwtConfig>()
                ?? throw new ArgumentNullException($"{nameof(JwtConfig)} must have a value.");

            if(string.IsNullOrWhiteSpace(jwtConfig.Secret))
            {
                string jwtSecret = configuration[IdentityConstants.JwtSecretKeyName] ?? throw new NullReferenceException("Jwt secret must have a value.");
                jwtConfig.Secret = jwtSecret;
            }

            services.AddAuthentication(opt =>
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
                        ValidAudience = jwtConfig.Audience,
                        ValidIssuer = jwtConfig.Issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret))
                    };
                });
        }

        public static void ConfigureAzureClients(this IServiceCollection services, IConfiguration configuration)
        {
            var keyVaultConfig = configuration.GetSection(IdentityConstants.KeyVaultSectionName).Get<KeyVaultConfig>()
                ?? throw new NullReferenceException($"{nameof(KeyVaultConfig)} must have a value.");

            // You can also reduce the number of calls to Azure Key Vault by caching your SecretClient
            // or any other Key Vault SDK client.
            // clients are designed to reuse an HttpClient by default and cache authentication bearer tokens for service like Key Vault
            // to reduce the number of calls to authenticate.
            services.AddAzureClients(config =>
            {
                // The DefaultAzureCredential chooses the best authentication mechanism based on your environment,
                // allowing you to move your app seamlessly from development to production with no code changes.
                // Enable Managed Service Identity for your Web App Service to be able to use Azure Key Vault
                // and authorize web app to access the Key Vault
                // Follow the link: https://www.loginradius.com/blog/engineering/guest-post/using-azure-key-vault-with-an-azure-web-app-in-c-sharp/
                config.UseCredential(new DefaultAzureCredential());

                // This will add SecretClient class to DI container which can be used in runtime to fetch data from AzureKeyVault
                config.AddSecretClient(new Uri(keyVaultConfig.KeyVaultUrl));
            });
        }
    }
}
