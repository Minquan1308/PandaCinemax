using DoAnCoSoTL.Models;
using DoAnCoSoTL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Repositories
{
    public class SeatRepository : ISeatRepository
    {
        MovieContext db;
        public SeatRepository(MovieContext _db)
        {
            db = _db;
        }
        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Seat>> GetAllAsync()
        {
            return await db.Seats.ToListAsync();
        }
        public async Task<Seat> GetByIdAsync(int id)
        {
            return await db.Seats.FindAsync(id);
        }
        
    }
}
