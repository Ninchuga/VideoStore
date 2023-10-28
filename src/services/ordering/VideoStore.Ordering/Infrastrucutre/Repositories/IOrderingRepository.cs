using VideoStore.Ordering.Models;

namespace VideoStore.Ordering.Infrastrucutre.Repositories
{
    public interface IOrderingRepository
    {
        Task<IReadOnlyList<Order>> GetOrders();
        Task<Order> GetOrderBy(int id);
        void DeleteOrder(Order movie);
        void AddOrder(Order movie);
        void UpdateOrder(Order movie);

        Task SaveChanges();
    }
}