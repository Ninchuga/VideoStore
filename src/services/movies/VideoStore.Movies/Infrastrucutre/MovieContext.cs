using Microsoft.EntityFrameworkCore;
using VideoStore.Movies.Models;

namespace VideoStore.Movies.Infrastrucutre
{
    public class MovieContext : DbContext
    {
        public MovieContext(DbContextOptions<MovieContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>(action =>
            {
                action.HasKey(movie => movie.Id);
                action.Property(movie => movie.Id)
                    .ValueGeneratedOnAdd()
                    .IsRequired();
            });
        }

        public DbSet<Movie> Movies { get; set; } = null!;

        public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
