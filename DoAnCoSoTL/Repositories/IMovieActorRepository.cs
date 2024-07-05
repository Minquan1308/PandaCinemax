using DoAnCoSoTL.Models;

namespace DoAnCoSoTL.Repositories
{
    public interface IMovieActorRepository
    {
        Task<IEnumerable<MovieActor>> GetAllAsync();
        //Task InsertAsync(MovieActor actor);
        //Task InsertAsync(MovieActor actor);
        Task DeletetAsync(int id);
        Task DeleteByMovieIdAsync(Guid id);
        Task InsertMovieActorAsync(Guid movieId, int actorId);

    }
}
