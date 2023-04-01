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

        public DbSet<Movie> Movies { get; set; } = null!;

        public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
