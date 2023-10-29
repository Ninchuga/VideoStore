using Azure.Identity;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using VideoStore.Bus;
using VideoStore.Ordering.Constants;
using VideoStore.Ordering.Handlers;
using VideoStore.Ordering.Infrastrucutre;
using VideoStore.Ordering.Infrastrucutre.Repositories;
using VideoStore.Ordering.Models;
using VideoStore.Shared;

namespace VideoStore.Ordering.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureDbContext(this IServiceCollection services, IHostBuilder host, IConfiguration configuration, IWebHostEnvironment environment)
        {
            bool useInMemoryDb = configuration.GetValue<bool>(OrderingConstants.FeatureFlags.UseInMemoryDatabase);
            if (useInMemoryDb)
            {
                services.AddDbContext<OrderingContext>(options =>
                    options.UseInMemoryDatabase("OrderingDb"));
                var serviceProvider = services.BuildServiceProvider();
                var context = serviceProvider.GetService<OrderingContext>();
                var logger = serviceProvider.GetService<ILogger<OrderingContextSeed>>();
                OrderingContextSeed.SeedAsync(context, logger).Wait();
            }
            else
            {
                services.AddDbContext<OrderingContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString(OrderingConstants.OrderingConnectionStringKey), option =>
                            {
                                option.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
                                // EF connection resiliency
                                option.EnableRetryOnFailure(
                                    maxRetryCount: 10,
                                    maxRetryDelay: TimeSpan.FromSeconds(10),
                                    errorNumbersToAdd: null);
                            }));

                host.MigrateDatabase<OrderingContext>(services, (context, services) =>
                {
                    var logger = services.GetService<ILogger<OrderingContextSeed>>();
                    OrderingContextSeed.SeedAsync(context, logger).Wait();
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
                    Title = "Ordering API",
                    Description = "Video store ordering WebAPI"
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
            var jwtConfig = configuration.GetSection(OrderingConstants.JwtConfigurationName).Get<JwtConfig>()
                ?? throw new NullReferenceException($"{nameof(JwtConfig)} must have a value.");

            if (string.IsNullOrWhiteSpace(jwtConfig.Secret))
            {
                string jwtSecret = configuration[OrderingConstants.JwtSecretKeyName] ?? throw new NullReferenceException("Jwt secret must have a value.");
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

        public static void ConfigureServiceBus(this IServiceCollection services, IConfiguration configuration, Serilog.ILogger logger)
        {
            string serviceBusConnectionString = configuration[OrderingConstants.AzureServiceBusConnectionStringKey];
            if(string.IsNullOrWhiteSpace(serviceBusConnectionString))
            {
                logger.Warning("Service bus connection string not found. Cannot configure service bus.");
                return;
            }

            services.AddMassTransit(config =>
            {
                config.AddConsumer<OrderMovieMessageHandler>();

                config.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(serviceBusConnectionString);

                    // We can configure duplication and message retries on a global level for all queues and topics
                    //cfg.UseMessageRetry(r => r.Interval(retryCount: 3, TimeSpan.FromSeconds(5)));
                    //cfg.EnableDuplicateDetection(TimeSpan.FromMinutes(10)); // default is 10 minutes

                    // This configures message queue (not topic)
                    cfg.ReceiveEndpoint(ServiceBusConstants.OrderMovieQueueName, endpoint =>
                    {
                        //endpoint.EnableDuplicateDetection(TimeSpan.FromMinutes(10)); // default is 10 minutes

                        // after specified number of retry attempts in case of exception
                        // message will be sent to _error queue
                        endpoint.UseMessageRetry(r =>
                        {
                            r.Ignore<ArgumentNullException>();
                            r.Interval(retryCount: 3, TimeSpan.FromSeconds(5));
                        });

                        endpoint.ConfigureConsumer<OrderMovieMessageHandler>(context);

                        endpoint.LockDuration = TimeSpan.FromMinutes(3); // defaults to 5

                        // How many times the transport will redeliver the message on negative acknowledgement
                        // This is different from retry, this is the transport redelivering the message to a receive endpoint
                        // before moving it to the dead letter
                        endpoint.MaxDeliveryCount = 3;
                    });
                });
            });
        }

        public static void AddRedisCaching(this IServiceCollection services, IConfiguration configuration, Serilog.ILogger logger)
        {
            string redisConnectionString = configuration[OrderingConstants.RedisConnectionStringKey];
            if(string.IsNullOrWhiteSpace(redisConnectionString))
                logger.Warning("Redis connection string not found.");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = OrderingConstants.RedisMessagingStoreInstanceName;
            });

            services.AddTransient<IMessageHandlersRepository, MessageHandlersRepository>();
        }

        public static void ConfigureAzureClients(this IServiceCollection services, IConfiguration configuration)
        {
            var keyVaultConfig = configuration.GetSection(OrderingConstants.KeyVaultSectionName).Get<KeyVaultConfig>()
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
