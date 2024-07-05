using DoAnCoSoTL.Models;

namespace DoAnCoSoTL.Repositories
{

    public interface ICartRepository
    {
        Task<IEnumerable<Cart>> GetAllAsync();
        Task<IEnumerable<Cart>> GetDataAsync(Cart cart);
        Task InsertAsync(Cart newCart);
        Task UpdateAsync(Cart editCart);
        Task DeleteAsync(int id);
    }
}
