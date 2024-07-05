using DoAnCoSoTL.Models;
using DoAnCoSoTL.ViewModels;

namespace DoAnCoSoTL.Repositories
{
    public interface IMovieRepository
    {
        Task Update(MovieViewModel editMovie, Guid Mid, IFormFile Image);
        MovieViewModel GetMovieByIdAdmin(Guid id);
        Task<Movie> GetByNameAsync(string name);
        string GetNameById(Guid id);
        Task<IEnumerable<Movie>> GetAllAsync();
        Task<Movie> GetByIdAsync(Guid id);
        Task InsertAsync(Movie movievm);
        Task UpdateAsync(Movie editMovie);
        Task DeleteAsync(Guid id);
        Task SaveChangesAsync();
        Task<IEnumerable<Movie>> SearchAsync(string keyword);
        Task<IEnumerable<Movie>> GetProductByCategoryAsync(int id);
        Task<int> TotalQuantitySoldAsync(Guid movieId);
        Task UpdateAsync(MovieViewModel editMovie, Guid id, IFormFile Image);
    }
}