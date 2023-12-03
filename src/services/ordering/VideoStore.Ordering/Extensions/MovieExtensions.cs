using VideoStore.Ordering.Models;
using VideoStore.Ordering.Models.Entities;

namespace VideoStore.Ordering.Extensions
{
    public static class MovieExtensions
    {
        public static Movie Map(this OrderMovie movie)
        {
            return new Movie
            {
                MovieRefId = movie.Id,
                Price = movie.Price,
                Title = movie.Title
            };
        }

        public static List<Movie> Map(this IEnumerable<OrderMovie> movies) =>
            movies.Select(movie => movie.Map()).ToList();

        public static List<Movie> Map(this IEnumerable<Bus.Messages.Movie> movies)
        {
            return movies.Select(movie => new Movie
            {
                MovieRefId = movie.Id,
                Title = movie.Title
            }).ToList();
        }
    }
}
