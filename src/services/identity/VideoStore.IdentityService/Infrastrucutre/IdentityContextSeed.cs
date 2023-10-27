using VideoStore.IdentityService.Model;

namespace VideoStore.IdentityService.Infrastrucutre
{
    public class IdentityContextSeed
    {
        public static async Task SeedAsync(IdentityContext context, ILogger<IdentityContextSeed>? logger)
        {
            var users = context.Users;
            if (!users.Any())
            {
                users.AddRange(GetPreconfiguredUsers());
                await context.SaveChanges();
                logger?.LogInformation("Seed database associated with context {DbContextName}", nameof(IdentityContext));
            }
        }

        private static IEnumerable<User> GetPreconfiguredUsers()
        {
            return new List<User>
            {
                new User { UserName = "nino", Password = "nino123", Email = "ninoemail@gmail.com" },
                new User { UserName = "donald", Password = "donald123", Email = "donaldemail@gmail.com" }
            };
        }
    }
}
