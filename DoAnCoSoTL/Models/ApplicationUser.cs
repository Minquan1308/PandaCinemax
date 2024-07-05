using DoAnCoSoTL.Models;
using Microsoft.AspNetCore.Identity;

namespace DoAnCoSoTL.Models
{


    public class ApplicationUser : IdentityUser
    {
        // Thêm các thuộc tính tùy chỉnh của người dùng
        public string? Role { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Image { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public virtual List<SeatOrder> SeatOrders { get; set; }
        public virtual List<Cart> Carts { get; set; }

        // Các thuộc tính khác tùy thuộc vào yêu cầu của ứng dụng

        // Thêm các liên kết đến các quan hệ khác
        //public virtual ICollection<MovieOrder> Orders { get; set; }
    }
}