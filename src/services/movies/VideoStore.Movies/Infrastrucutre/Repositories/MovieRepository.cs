using Microsoft.EntityFrameworkCore;
using VideoStore.Movies.Models;

namespace VideoStore.Movies.Infrastrucutre.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly MovieContext _dbContext;

        public MovieRepository(MovieContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void UpsertMovie(Movie movie) =>
            _dbContext.Movies.Update(movie);

        public void DeleteMovie(Movie movie) =>
            _dbContext.Movies.Remove(movie);

        public async Task<Movie> GetMovieBy(int id, CancellationToken cancellationToken) =>
            await _dbContext.Movies.FindAsync(id, cancellationToken);

        public async Task<IReadOnlyList<Movie>> GetMovies(CancellationToken cancellationToken) =>
            await _dbContext.Movies.AsNoTracking().ToListAsync(cancellationToken);

        public async Task SaveChanges(CancellationToken cancellationToken) =>
            await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
