using VideoStore.Movies.DTOs;
using VideoStore.Movies.Models;

namespace VideoStore.Movies.Extensions
{
    public static class MovieExtensions
    {
        public static Movie ToEntity(this MovieDTO movie) =>
            new() { Genre = movie.Genre, ReleaseDate = movie.ReleaseDate, Title = movie.Title, Price = movie.Price };

        public static List<Movie> ToEntities(this IEnumerable<MovieDTO> movies) =>
            movies.Select(x => x.ToEntity()).ToList();

        public static MovieDTO ToDto(this Movie movie) =>
            new(movie.Id, movie.Title, movie.Genre, movie.ReleaseDate, movie.Price);

        public static IReadOnlyList<MovieDTO> ToDtos(this IEnumerable<Movie> movies) =>
            movies.Select(x => x.ToDto()).ToList();

        public static Bus.Messages.Movie ToBusMessage(this Movie movie) =>
            new(movie.Id, movie.Title, movie.Price);

        public static IReadOnlyList<Bus.Messages.Movie> ToBusMessages(this IEnumerable<Movie> movies) =>
            movies.Select(x => x.ToBusMessage()).ToList();

        public static OrderMovie Map(this Movie movie) =>
            new(movie.Id, movie.Title, movie.Price);

        public static IReadOnlyList<OrderMovie> Map(this IEnumerable<Movie> movies) =>
            movies.Select(x => x.Map()).ToList();

    }
}
