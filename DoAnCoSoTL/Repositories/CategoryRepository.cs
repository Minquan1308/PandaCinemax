using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        MovieContext db;
        public CategoryRepository(MovieContext _db)
        {
            db = _db;
        }
     
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await db.Categories.ToListAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await db.Categories.FindAsync(id);
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await db.Categories.FindAsync(name);
        }

        public async Task InsertAsync(Category newCategory)
        {
            db.Categories.Add(newCategory);
            await db.SaveChangesAsync();
        }
        public async Task UpdateAsync(Category editCategory)
        {
            db.Categories.Update(editCategory);
            await db.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var category = await db.Categories.FindAsync(id);
            db.Categories.Remove(category);
            await db.SaveChangesAsync();
        }




    }
}
