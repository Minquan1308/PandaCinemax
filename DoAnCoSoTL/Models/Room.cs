using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCoSoTL.Models
{
    public class Room
    {
        public int Id { get; set; }
        [Required]
        public int soHang { get; set; }
        public int soCot { get; set; }

        [ForeignKey("Cinema")]
        public int CinemaId { get; set; }
        public virtual Cinema Cinema { get; set; }
        public ICollection<Screening> Screenings { get; set; }
    }
}
