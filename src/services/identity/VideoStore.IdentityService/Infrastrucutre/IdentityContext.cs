using Microsoft.EntityFrameworkCore;
using VideoStore.IdentityService.Model;

namespace VideoStore.IdentityService.Infrastrucutre
{
    public class IdentityContext : DbContext
    {
        public IdentityContext(DbContextOptions<IdentityContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(action =>
            {
                action.HasKey( user =>  user.Id);
                action.Property( user => user.Id)
                    .ValueGeneratedOnAdd()
                    .IsRequired();
            });
        }

        public DbSet<User> Users { get; set; } = null!;

        public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
