using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoAnCoSoTL.Models
{
    public class Producer
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public string? Image { get; set; }
        [Required(ErrorMessage = "Bio is required")]
        public string Bio { get; set; }
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
