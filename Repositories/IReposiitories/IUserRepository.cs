using MangoStore_API.Models;

namespace MangoStore_API.Repositories.IReposiitories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> UpdateAsync(User user);
    }
}
