using DoAnCoSoTL.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Repositories
{ 
    public class MovieActorRepository : IMovieActorRepository
    {
        MovieContext db;
        public MovieActorRepository(MovieContext db)
        {
           this.db = db;
        }
        public async Task<IEnumerable<MovieActor>> GetAllAsync()
        {
            return await db.MovieActors.ToListAsync();

        }
        public async Task InsertMovieActorAsync(Guid movieId, int actorId)
        {
            var movieActor = new MovieActor
            {
                MovieId = movieId,
                ActorId = actorId
            };

            await db.MovieActors.AddAsync(movieActor);
            await db.SaveChangesAsync();
        }
        //public async Task InsertAsync(MovieActor actor)
        //{
        //    db.MovieActors.Add(actor);
        //    await db.SaveChangesAsync();
        //}
        public async Task DeletetAsync(int id)
        {
            var movie = db.MovieActors.FirstOrDefault(a => a.Id == id);
            db.MovieActors.Remove(movie);   
            await db.SaveChangesAsync();
        }
        public async Task DeleteByMovieIdAsync(Guid id)
        {
            var movieActors = db.MovieActors.Where(movieActor => movieActor.MovieId == id);
            db.MovieActors.RemoveRange(movieActors);
            await db.SaveChangesAsync();
        }



    }
}
