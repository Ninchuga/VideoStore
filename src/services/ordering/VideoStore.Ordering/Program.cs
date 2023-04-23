using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using VideoStore.Bus;
using VideoStore.Movies.Infrastrucutre;
using VideoStore.Movies.Infrastrucutre.Repositories;
using VideoStore.Ordering.Constants;
using VideoStore.Ordering.Handlers;
using VideoStore.Ordering.Models;
using VideoStore.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); // remove default logging providers
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // clear Microsoft changed claim names from dictionary and preserve original ones

try
{
    builder.Logging.AddSerilog(LoggingConfiguration.CreateLogger(builder.Environment));
    var configuration = GetConfiguration(builder.Environment);

    Log.Information("Configuring web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    ConfigureDbContext(builder, configuration);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    ConfigureSwagger(builder);
    builder.Services.AddTransient<IOrderingRepository, OrderingRepository>();
    builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(OrderingConstants.JwtConfigurationName));
    builder.Services.AddTransient(typeof(IIdempotentMessageHandler<>), typeof(IdempotentMessageHandlerDecorator<>));
    ConfigureAuthentication(builder);
    ConfigureServiceBus(builder, configuration);

    Log.Information("Starting web host ({ApplicationContext})...", builder.Environment.ApplicationName);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/V1/swagger.json", "Ordering WebAPI");
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
    builder.Services.AddDbContext<OrderingContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString(OrderingConstants.OrderingConnectionStringKey), option =>
                    {
                        option.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
                        // EF connection resiliency
                        option.EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    }));
}

void ConfigureSwagger(WebApplicationBuilder builder)
{
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("V1", new OpenApiInfo
        {
            Version = "V1",
            Title = "Ordering API",
            Description = "Ordering WebAPI"
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

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    var jwtConfig = builder.Configuration.GetSection(OrderingConstants.JwtConfigurationName).Get<JwtConfig>();

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

static void ConfigureServiceBus(WebApplicationBuilder builder, IConfiguration configuration)
{
    builder.Services.AddMassTransit(config =>
    {
        config.AddConsumer<OrderMovieMessageHandler>();

        config.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(configuration.GetConnectionString("AzureServiceBusConnectionString"));

            // We can configure duplication and message retries on a global level for all queues and topics
            //cfg.UseMessageRetry(r => r.Interval(retryCount: 3, TimeSpan.FromSeconds(5)));
            //cfg.EnableDuplicateDetection(TimeSpan.FromMinutes(10)); // default is 10 minutes

            // This configures message queue (not topic)
            cfg.ReceiveEndpoint(ServiceBusConstants.OrderMovieQueueName, endpoint =>
            {
                endpoint.EnableDuplicateDetection(TimeSpan.FromMinutes(10)); // default is 10 minutes

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