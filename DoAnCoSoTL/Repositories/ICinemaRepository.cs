using DoAnCoSoTL.Models;

namespace DoAnCoSoTL.Repositories
{
    public interface ICinemaRepository
    {
        Task<Cinema> GetByNameAsync(string name);
        Task<IEnumerable<Cinema>> GetAllAsync();
        Task<Cinema> GetByIdAsync(int id);
        Task<Cinema> GetByLocationAsync(string location);
        Task InsertAsync(Cinema newCinema);

        Task UpdateAsync(Cinema editCinema);
        Task DeleteAsync(int id);
    }
    
}