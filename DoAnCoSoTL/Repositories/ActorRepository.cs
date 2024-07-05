using DoAnCoSoTL.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Repositories
{
    public class ActorRepository : IActorRepository
    {


        MovieContext db;
        public ActorRepository(MovieContext _db)
        {
            db = _db;
        }
        public async Task<IEnumerable<Actor>> GetAllAsync()
        {
            return await db.Actors.ToListAsync();
        }
        public async Task<Actor> GetByIdAsync(int id)
        {
            return await db.Actors.SingleOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Actor> GetByNameAsync(string name)
        {
            return await db.Actors.FindAsync(name);
        }
        public async Task InsertAsync(Actor actor)
        {
            db.Actors.Add(actor);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Actor newActor)
        {

            db.Actors.Update(newActor);
            await db.SaveChangesAsync();
        }


        public async Task DeleteAsync(int id)
        {
            var actor = await db.Actors.SingleOrDefaultAsync(p => p.Id == id);
            db.Actors.Remove(actor);
            await db.SaveChangesAsync();


        }
    }
}
