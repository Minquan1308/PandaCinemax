using DoAnCoSoTL.Models;

namespace DoAnCoSoTL.Repositories
{
    public interface IScreeningRepository
    {
        Task InsertScreeningAsync(Screening screening);
        Task UpdateScreeningAsync(Screening editScreening);
        //IEnumerable<Cinema> GetSelectedCinemasByMovieId(Guid movieId);
        Task<IEnumerable<Screening>> GetAllAsync();
        Task<IEnumerable<Screening>> GetScreeningsByDate(DateTime date);
        Task<Screening> GetByIdAsync(int id);
        Task DeleteAsync(int id);
        Task<IEnumerable<Screening>> GetByMovieId(Guid id);
    }
}
