using VideoStore.Movies.Models;

namespace VideoStore.Movies.Infrastrucutre.Repositories
{
    public interface IMovieRepository
    {
        Task<IReadOnlyList<Movie>> GetMovies();
        Task<Movie> GetMovieBy(int id);
        void DeleteMovie(Movie movie);
        void AddMovie(Movie movie);
        void UpdateMovie(Movie movie);

        Task SaveChanges();
    }
}