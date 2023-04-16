using Microsoft.EntityFrameworkCore;
using VideoStore.IdentityService.Model;

namespace VideoStore.IdentityService.Infrastrucutre.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IdentityContext _dbContext;

        public UserRepository(IdentityContext context)
        {
            _dbContext = context;
        }

        public void AddUser(User user) =>
            _dbContext.Users.Add(user);

        public async Task<User> FindUser(User user) =>
            await _dbContext.Users.FirstOrDefaultAsync(dbUser =>
                dbUser.UserName.Equals(user.UserName) &&
                dbUser.Password.Equals(user.Password));

        public async Task<IReadOnlyList<User>> GetAllUsers() =>
            await _dbContext.Users.ToListAsync();

        public async Task SaveChanges() =>
            await _dbContext.SaveChangesAsync();
    }
}
