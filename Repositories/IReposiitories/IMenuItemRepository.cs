using MangoStore_API.Models;

namespace MangoStore_API.Repositories.IReposiitories
{
    public interface IMenuItemRepository : IRepository<MenuItem>
    {
        Task<MenuItem> UpdateAsync(MenuItem item);
    }
}
