using Microsoft.EntityFrameworkCore;
using VideoStore.Ordering.Models.Entities;

namespace VideoStore.Ordering.Infrastrucutre.Repositories
{
    public class OrderingRepository : IOrderingRepository
    {
        private readonly OrderingContext _dbContext;

        public OrderingRepository(OrderingContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddOrder(Order movie) =>
            _dbContext.Orders.Add(movie);

        public void UpdateOrder(Order movie) =>
            _dbContext.Update(movie);

        public void DeleteOrder(Order movie) =>
            _dbContext.Orders.Remove(movie);

        public async Task<Order> GetOrderBy(int id) =>
            await _dbContext.Orders.FindAsync(id);

        public async Task<IReadOnlyList<Order>> GetOrders() =>
            await _dbContext.Orders.AsNoTracking().ToListAsync();

        public async Task SaveChanges() =>
            await _dbContext.SaveChangesAsync();
    }
}
