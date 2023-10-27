using VideoStore.Movies.Models;

namespace VideoStore.Movies.Infrastrucutre.Repositories
{
    public interface IMovieRepository
    {
        Task<IReadOnlyList<Movie>> GetMovies(CancellationToken cancellationToken);
        Task<Movie> GetMovieBy(int id, CancellationToken cancellationToken);
        void DeleteMovie(Movie movie);
        void UpsertMovie(Movie movie);
        Task SaveChanges(CancellationToken cancellationToken);
    }
}