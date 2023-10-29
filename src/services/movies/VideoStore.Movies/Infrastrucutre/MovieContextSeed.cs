using VideoStore.Movies.Models;

namespace VideoStore.Movies.Infrastrucutre
{
    public class MovieContextSeed
    {
        public static async Task SeedAsync(MovieContext context, ILogger<MovieContextSeed>? logger)
        {
            if (context is null)
            {
                logger?.LogError("Context {Context} cannot be null while executing {ClassName}{MethodName}",
                    nameof(MovieContext), nameof(MovieContextSeed), nameof(SeedAsync));
                return;
            }

            var movies = context.Movies;
            if (!movies.Any())
            {
                movies.AddRange(GetPreconfiguredMovies());
                await context.SaveChanges();
                logger?.LogInformation("Seed database associated with context {DbContextName}", nameof(MovieContext));
            }
        }

        private static IEnumerable<Movie> GetPreconfiguredMovies()
        {
            return new List<Movie>
            {
                new Movie
                {
                    Genre = "Comedy",
                    Title = "Ace Ventura",
                    ReleaseDate = new DateTime(1994, 2, 4),
                    Price = 30
                },
                new Movie
                {
                    Genre = "Action",
                    Title = "Terminator",
                    ReleaseDate = new DateTime(1985, 2, 21),
                    Price = 25
                }
            };
        }
    }
}
