using Microsoft.EntityFrameworkCore;
using VideoStore.Ordering.Models;

namespace VideoStore.Movies.Infrastrucutre
{
    public class OrderingContext : DbContext
    {
        public OrderingContext(DbContextOptions<OrderingContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(action =>
            {
                action.HasKey(movie => movie.Id);
                action.Property(movie => movie.Id)
                    .ValueGeneratedOnAdd()
                    .IsRequired();
            });
        }

        public DbSet<Order> Orders { get; set; } = null!;

        public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
