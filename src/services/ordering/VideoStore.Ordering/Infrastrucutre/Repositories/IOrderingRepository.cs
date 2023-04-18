using VideoStore.Ordering.Models;

namespace VideoStore.Movies.Infrastrucutre.Repositories
{
    public interface IOrderingRepository
    {
        Task<IReadOnlyList<Order>> GetMovies();
        Task<Order> GetMovieBy(int id);
        void DeleteMovie(Order movie);
        void AddMovie(Order movie);
        void UpdateMovie(Order movie);

        Task SaveChanges();
    }
}