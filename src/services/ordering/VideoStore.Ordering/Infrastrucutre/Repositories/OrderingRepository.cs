using Microsoft.EntityFrameworkCore;
using VideoStore.Ordering.Models;

namespace VideoStore.Movies.Infrastrucutre.Repositories
{
    public class OrderingRepository : IOrderingRepository
    {
        private readonly OrderingContext _dbContext;

        public OrderingRepository(OrderingContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddMovie(Order movie) =>
            _dbContext.Orders.Add(movie);

        public void UpdateMovie(Order movie) =>
            _dbContext.Entry(movie).State = EntityState.Modified;

        public void DeleteMovie(Order movie) =>
            _dbContext.Orders.Remove(movie);

        public async Task<Order> GetMovieBy(int id) =>
            await _dbContext.Orders.FindAsync(id);

        public async Task<IReadOnlyList<Order>> GetMovies() =>
            await _dbContext.Orders.AsNoTracking().ToListAsync();

        public async Task SaveChanges() =>
            await _dbContext.SaveChangesAsync();
    }
}
