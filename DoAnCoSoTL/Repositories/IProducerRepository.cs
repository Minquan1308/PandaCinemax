using DoAnCoSoTL.Models;

namespace DoAnCoSoTL.Repositories
{
    public interface IProducerRepository
    {
        Task<Producer> GetByNameAsync(string name);
        Task<IEnumerable<Producer>> GetAllAsync();
        Task<Producer> GetByIdAsync(int id);
        Task InsertAsync(Producer newProducer);
        Task UpdateAsync(Producer editProduce);
        Task DeleteAsync(int id);
    }
}