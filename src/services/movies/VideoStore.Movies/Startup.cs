using Microsoft.EntityFrameworkCore;
using System.Reflection;
using VideoStore.Movies.Infrastrucutre;

namespace VideoStore.Movies
{
    // NOT USED CURRENTLY
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MovieContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("OrderingConnectionString"), option =>
                {
                    option.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                    // EF connection resiliency
                    option.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                }));

            services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
