using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using DoAnCoSoTL.ViewModels;
using Microsoft.EntityFrameworkCore;

public class ScreeningRepository : IScreeningRepository
{
    MovieContext db;
    public ScreeningRepository(MovieContext _db)
    {
        db = _db;
    }
    public async Task<Screening> GetByIdAsync(int id)
    {
        return await db.Screenings.FindAsync(id);
    }
    public async Task InsertScreeningAsync(Screening screening)
    {
        db.Screenings.Add(screening);
        await db.SaveChangesAsync();
    }
    public async Task UpdateScreeningAsync(Screening editScreening)
    {
        db.Screenings.Update(editScreening);
        await db.SaveChangesAsync();
    }
    public async Task DeleteAsync(int id)
    {
        var screening = await db.Screenings.FindAsync(id);
        db.Screenings.Remove(screening);
        await db.SaveChangesAsync();
    }
    public async Task<IEnumerable<Screening>> GetAllAsync()
    {
        return await db.Screenings.ToListAsync();
    }
    public async Task<IEnumerable<Screening>> GetScreeningsByDate(DateTime date)
    {
        return db.Screenings.Where(s => s.Date == date.Date).ToList();
    }
    public async Task<IEnumerable<Screening>> GetByMovieId(Guid movieId)
    {
        return db.Screenings.Where(p => p.MovieId == movieId).ToList();
    }




}