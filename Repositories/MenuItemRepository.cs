using MangoStore_API.Data;
using MangoStore_API.Models;
using MangoStore_API.Repositories.IReposiitories;

namespace MangoStore_API.Repositories
{
    public class MenuItemRepository : Repository<MenuItem>, IMenuItemRepository
    {
        private readonly ApplicationDbContext _db;
        public MenuItemRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<MenuItem> UpdateAsync(MenuItem item)
        {
            _db.MenuItems.Update(item);
            await _db.SaveChangesAsync();
            return item;
        }
    }
}
