using DoAnCoSoTL.Models;

namespace DoAnCoSoTL.Repositories
{
    public interface IActorRepository
    {
       
        Task DeleteAsync(int id);
        Task<IEnumerable<Actor>> GetAllAsync();
        Task <Actor> GetByIdAsync(int id);
        Task <Actor> GetByNameAsync(string name);
        Task InsertAsync(Actor newActor);
        Task UpdateAsync(Actor EditActor);
    }
}