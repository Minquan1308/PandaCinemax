using DoAnCoSoTL.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Repositories
{
    public class ProducerRepository : IProducerRepository
    {
        MovieContext db;
        public ProducerRepository(MovieContext _db)
        {
            db = _db;
        }

        public async Task<IEnumerable<Producer>> GetAllAsync()
        {
            return await db.Producers.ToListAsync();
        }
        public async Task<Producer> GetByIdAsync(int id)
        {
            return await db.Producers.FindAsync(id);
        }

        public async Task<Producer> GetByNameAsync(string name)
        {
            return await db.Producers.FindAsync(name);
        }

        public async Task InsertAsync(Producer newProducer)
        {
            db.Producers.Add(newProducer);
            await db.SaveChangesAsync();
        }
        public async Task UpdateAsync(Producer editProducer)
        {
            db.Producers.Update(editProducer);
            await db.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var producer = await db.Producers.FindAsync(id);
            db.Producers.Remove(producer);
            await db.SaveChangesAsync();
        }




    }
}
