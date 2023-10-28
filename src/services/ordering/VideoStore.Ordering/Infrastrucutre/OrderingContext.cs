using Microsoft.EntityFrameworkCore;
using VideoStore.Ordering.Models;

namespace VideoStore.Ordering.Infrastrucutre
{
    public class OrderingContext : DbContext
    {
        public OrderingContext(DbContextOptions<OrderingContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(orderEntity =>
            {
                orderEntity.HasKey(order => order.Id);
                orderEntity.Property(order => order.Id)
                    .ValueGeneratedOnAdd()
                    .IsRequired();

                orderEntity.OwnsMany(order => order.Movies,
                builder =>
                {
                    builder.ToJson();
                });
            });

            modelBuilder.Entity<IdempotentConsumer>()
                .HasKey(x => new { x.MessageId, x.Consumer });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
            .LogTo(Console.WriteLine, (_, level) => level == LogLevel.Information)
            .EnableSensitiveDataLogging();
        }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<IdempotentConsumer> IdempotentConsumers => Set<IdempotentConsumer>();

        public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
