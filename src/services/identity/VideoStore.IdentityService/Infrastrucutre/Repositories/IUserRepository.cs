using VideoStore.IdentityService.Model;

namespace VideoStore.IdentityService.Infrastrucutre.Repositories
{
    public interface IUserRepository
    {
        void AddUser(User user);
        Task<User> FindUser(User user);
        Task<IReadOnlyList<User>> GetAllUsers();

        Task SaveChanges();
    }
}