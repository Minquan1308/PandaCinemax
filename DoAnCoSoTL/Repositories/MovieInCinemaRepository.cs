using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Repositories
{
    public class MovieInCinemaRepository : IMovieInCinemaRepository
    {
        MovieContext db;
        public MovieInCinemaRepository(MovieContext db)
        {
            this.db = db;
        }
        public async Task InsertMoviesInCinemaAsync(Guid movieId, int cinemaId)
        {
            var movieInCinema = new MoviesInCinema
            {
                MovieId = movieId,
                CinemaId = cinemaId
            };

            await db.MovieInCinemas.AddAsync(movieInCinema);
            await db.SaveChangesAsync();
        }
        public async Task<IEnumerable<MoviesInCinema>> GetAllAsync()
        {
            return await db.MovieInCinemas.ToListAsync();
        }


        public async Task<IEnumerable<Cinema>> GetCinemasByMovieId(Guid movieId)
        {
            return await db.MovieInCinemas
                .Where(mc => mc.MovieId == movieId)
                .Select(mc => mc.Cinema)
                .ToListAsync();
        }


        //public async Task InsertAsync(IEnumerable<MoviesInCinema> mic)
        //{
        //    await db.MovieInCinemas.AddRangeAsync(mic);
        //    await db.SaveChangesAsync();
        //}

        public async Task Update(int id, MoviesInCinema mic)
        {
            var oldMovie = db.MovieInCinemas.SingleOrDefault(w => w.Id == id);
            await db.SaveChangesAsync();

        }

        public async Task Delete(int id)
        {
            var movie = db.MovieInCinemas.SingleOrDefault(w => w.Id == id);
            db.MovieInCinemas.Remove(movie);
            await db.SaveChangesAsync();
        }
        public async Task DeleteByMovieIdAsync(Guid id)
        {
            var movie = db.MovieInCinemas.Where(x => x.MovieId == id).ToList();
            db.MovieInCinemas.RemoveRange(movie);
            await db.SaveChangesAsync();
        }
    }
}
