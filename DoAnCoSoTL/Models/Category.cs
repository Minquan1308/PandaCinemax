using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoAnCoSoTL.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string? Image { get; set; }
        public ICollection<Movie> Movies { get; set; }
    }
}
