using DoAnCoSoTL.Models;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Repositories
{
    public class CinemaRepository : ICinemaRepository
    {
        MovieContext db;
        public CinemaRepository(MovieContext _db)
        {
            db = _db;
        }

        public async Task<IEnumerable<Cinema>> GetAllAsync()
        {
            return await db.Cinemas.ToListAsync();
        }
        public async Task<Cinema> GetByIdAsync(int id)
        {
            return await db.Cinemas.FindAsync(id);
        }

        public async Task<Cinema> GetByNameAsync(string name)
        {
            return await db.Cinemas.FirstOrDefaultAsync(c => c.Name == name);
        }


        public async Task InsertAsync(Cinema newCinema)
        {
            db.Cinemas.Add(newCinema);
            await db.SaveChangesAsync();
        }
        public async Task UpdateAsync(Cinema editCinema)
        {
            db.Cinemas.Update(editCinema);
            await db.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var cinema = await db.Cinemas.FindAsync(id);
            db.Cinemas.Remove(cinema);
            await db.SaveChangesAsync();
        }
        public async Task<Cinema> GetByLocationAsync(string location)
        {
            return await db.Cinemas.FindAsync(location);
        }




    }
}