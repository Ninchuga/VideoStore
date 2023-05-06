using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using VideoStore.IdentityService.Constants;
using VideoStore.IdentityService.Infrastrucutre;
using VideoStore.Shared;

namespace VideoStore.Movies.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureDbContext(this IServiceCollection services, ConfigureHostBuilder host, IConfiguration configuration)
        {
            services.AddDbContext<IdentityContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString(IdentityConstants.IdentityConnectionStringKey), option =>
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
    }
}
