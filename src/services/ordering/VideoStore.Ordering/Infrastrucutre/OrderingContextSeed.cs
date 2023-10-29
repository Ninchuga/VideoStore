using VideoStore.Ordering.Models;

namespace VideoStore.Ordering.Infrastrucutre
{
    public class OrderingContextSeed
    {
        public static async Task SeedAsync(OrderingContext context, ILogger<OrderingContextSeed>? logger)
        {
            if (context is null)
            {
                logger?.LogError("Context {Context} cannot be null while executing {ClassName}{MethodName}",
                    nameof(OrderingContext), nameof(OrderingContextSeed), nameof(SeedAsync));
                return;
            }

            var movies = context.Orders;
            if (!movies.Any())
            {
                movies.AddRange(GetPreconfiguredOrders());
                await context.SaveChanges();
                logger?.LogInformation("Seed database associated with context {DbContextName}", nameof(OrderingContext));
            }
        }

        private static IEnumerable<Order> GetPreconfiguredOrders()
        {
            return new List<Order>
            {
                new Order
                {
                    UserEmail = "ninoemail@gmail.com",
                    UserName = "nino",
                    Created = DateTime.UtcNow,
                    Movies = new List<Movie>
                    {
                        new Movie
                        {
                            MovieRefId = 1,
                            Title = "Ace Ventura",
                            Price = 30
                        }
                    }
                },
                new Order
                {
                    UserEmail = "donaldemail@gmail.com",
                    UserName = "donald",
                    Created = DateTime.UtcNow,
                    Movies = new List<Movie>
                    {
                        new Movie
                        {
                            MovieRefId = 2,
                            Title = "Terminator",
                            Price = 25
                        }
                    }
                }
            };
        }
    }
}
