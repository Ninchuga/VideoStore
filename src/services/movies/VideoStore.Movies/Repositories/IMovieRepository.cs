using VideoStore.Movies.Models;

namespace VideoStore.Movies.Repositories
{
    public interface IMovieRepository
    {
        Task<List<Movie>> GetMovies();
        Task<Movie> GetMovieBy(int id);
        void DeleteMovie(Movie movie);
        void AddMovie(Movie movie);
        void UpdateMovie(Movie movie);

        Task SaveChanges();
    }
}