using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoAnCoSoTL.Models
{
    public class Actor
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Image { get; set; }
        [Required]
        public string Bio { get; set; }

        public virtual List<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
