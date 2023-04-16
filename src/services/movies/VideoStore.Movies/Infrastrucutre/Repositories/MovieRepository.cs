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

        public void AddMovie(Movie movie) =>
            _dbContext.Movies.Add(movie);

        public void UpdateMovie(Movie movie) =>
            _dbContext.Entry(movie).State = EntityState.Modified;

        public void DeleteMovie(Movie movie) =>
            _dbContext.Movies.Remove(movie);

        public async Task<Movie> GetMovieBy(int id) =>
            await _dbContext.Movies.FindAsync(id);

        public async Task<IReadOnlyList<Movie>> GetMovies() =>
            await _dbContext.Movies.ToListAsync();

        public async Task SaveChanges() =>
            await _dbContext.SaveChangesAsync();
    }
}
