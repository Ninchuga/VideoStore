using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using VideoStore.Movies.Constants;
using VideoStore.Movies.Infrastrucutre;
using VideoStore.Movies.Models;
using VideoStore.Shared;

namespace VideoStore.Movies.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureDbContext(this IServiceCollection services, ConfigureHostBuilder host, IConfiguration configuration)
        {
            services.AddDbContext<MovieContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString(MoviesConstants.MoviesConnectionStringKey), option =>
                            {
                                option.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
                                // EF connection resiliency
                                option.EnableRetryOnFailure(
                                    maxRetryCount: 10,
                                    maxRetryDelay: TimeSpan.FromSeconds(10),
                                    errorNumbersToAdd: null);
                            }));

            host.MigrateDatabase<MovieContext>(services, (context, services) =>
            {
                var logger = services.GetService<ILogger<MovieContextSeed>>();
                MovieContextSeed.SeedAsync(context, logger).Wait();
            });
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
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

        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtConfig = configuration.GetSection(MoviesConstants.JwtConfigurationName).Get<JwtConfig>();
            if (jwtConfig is null)
                throw new ArgumentNullException($"{nameof(JwtConfig)} must have a value.");
            if (string.IsNullOrWhiteSpace(jwtConfig.Secret))
                throw new ArgumentNullException($"{nameof(JwtConfig)} {nameof(JwtConfig.Secret)} must have a value.");

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

        public static void ConfigureServiceBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(config =>
            {
                config.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration.GetConnectionString(MoviesConstants.AzureServiceBusConnectionStringKey));
                });
            });
        }
    }
}
