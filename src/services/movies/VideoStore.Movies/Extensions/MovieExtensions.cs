using VideoStore.Movies.DTOs;
using VideoStore.Movies.Models;

namespace VideoStore.Movies.Extensions
{
    public static class MovieExtensions
    {
        public static Movie ToEntity(this MovieDTO movie) =>
            new() { Genre = movie.Genre, ReleaseDate = movie.ReleaseDate, Title = movie.Title, Price = movie.Price };

        public static Movie ToEntity(this MovieDTO movie, int movieId) =>
            new() { Id = movieId, Genre = movie.Genre, ReleaseDate = movie.ReleaseDate, Title = movie.Title, Price = movie.Price };

        public static List<Movie> ToEntities(this IEnumerable<MovieDTO> movies) =>
            movies.Select(x => x.ToEntity()).ToList();

        public static MovieDTO ToDto(this Movie movie) =>
            new() { Genre = movie.Genre, ReleaseDate = movie.ReleaseDate, Title = movie.Title, Price = movie.Price };

        public static IReadOnlyList<MovieDTO> ToDtos(this IEnumerable<Movie> movies) =>
            movies.Select(x => x.ToDto()).ToList();
    }
}
