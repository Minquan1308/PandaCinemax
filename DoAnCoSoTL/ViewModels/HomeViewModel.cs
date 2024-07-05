using DoAnCoSoTL.Models;

namespace DoAnCoSoTL.ViewModels
{
    public class HomeViewModel
    {
        public ApplicationUser user { get; set; }
        public List<Cinema> Cinemas { get; set; }
        public List<Movie> Movies { get; set; }
        public List<Category> Categories { get; set; }
        public List<Actor> Actors { get; set; }
    }
}
