using DoAnCoSoTL.Models;

namespace DoAnCoSoTL.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category> GetByNameAsync(string name);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task InsertAsync(Category newCinema);
        Task UpdateAsync(Category editMovie);
        Task DeleteAsync(int id);


    }
}
