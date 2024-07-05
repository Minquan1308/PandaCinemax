using DoAnCoSoTL.Models;

namespace DoAnCoSoTL.Repositories
{
    public interface IUpdateProfileRepository
    {
        ApplicationUser GetById(string id);
        Task<int> insert(ApplicationUser NewUser, IFormFile Image);
        Task<int> updateAsync(string id, ApplicationUser UpdateUser,IFormFile Image);
    }
}