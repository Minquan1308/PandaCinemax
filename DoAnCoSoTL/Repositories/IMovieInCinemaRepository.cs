using DoAnCoSoTL.Models;

namespace DoAnCoSoTL.Repositories
{
    public interface IMovieInCinemaRepository
    {
        Task<IEnumerable<MoviesInCinema>> GetAllAsync();
        Task<IEnumerable<Cinema>> GetCinemasByMovieId(Guid movieId);
        Task Update(int id, MoviesInCinema mic);
        Task Delete(int id);
        Task InsertMoviesInCinemaAsync(Guid movieId, int cinemaId);
        Task DeleteByMovieIdAsync(Guid id);
    }
}
