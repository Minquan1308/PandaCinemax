using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using DoAnCoSoTL.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Import thư viện này để sử dụng ToListAsync()

namespace DoAnCoSoTL.Controllers
{
    public class OrderListController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MovieContext _context;
        private readonly ICinemaRepository _cinemaRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderListController(UserManager<ApplicationUser> userManager, MovieContext context, ICinemaRepository cinemaRepository, IMovieRepository movieRepository, IOrderRepository orderRepository)
        {
            _userManager = userManager;
            _context = context;
            _cinemaRepository = cinemaRepository;
            _movieRepository = movieRepository;
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var list = await _context.SeatOrders
                .Include(s => s.SeatOrderDetails)
                .Where(s => s.UserId == user.Id.ToString())
                .OrderByDescending(s => s.OrderDate) // Sắp xếp theo thời gian từ mới nhất đến cũ nhất
                .ToListAsync();

            var orderHistory = new List<OrderHistoryViewModel>();

            foreach (var order in list)
            {
                foreach (var detail in order.SeatOrderDetails.GroupBy(s => new { s.CinemaName, s.MovieId}))
                {
                    // Lấy tên phim từ MovieRepository
                    //var movieName = await _movieRepository.GetNameById(detail.Key.MovieId);

                    // Thêm thông tin vào danh sách orderHistory
                    orderHistory.Add(new OrderHistoryViewModel
                    {
                        OrderId = order.Id,
                        CinemaName = detail.Key.CinemaName,
                        MovieName = _movieRepository.GetNameById(detail.Key.MovieId),
                        OrderDate = order.OrderDate,

                    });
                }
            }
            return View(orderHistory);
        }
        public async Task<IActionResult> Detail(Guid orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var details = await _context.SeatOrderDetails
                                        .Where(s => s.SeatOrderId == orderId)
                                        .ToListAsync();

            if (details == null || details.Count == 0)
            {
                return NotFound();
            }
            var seatOrder = await _context.SeatOrders.FindAsync(orderId);
            if (seatOrder == null)
            {
                return NotFound();
            }

            var movie = await _movieRepository.GetByIdAsync(details.First().MovieId);
            ViewData["SoLuong"] = seatOrder.TotalQuantitySold;
            ViewData["TongTien"] = seatOrder.TotalPrice;
            ViewData["TenPhim"] = movie.Name;
            ViewData["Poster"] = movie.Image;
            ViewData["OrderDate"] = seatOrder.OrderDate.ToShortDateString();
            ViewData["RoomNumber"] = details.First().RoomId;
            ViewData["CinemaName"] = details.First().CinemaName;
            ViewData["TimeSlot"] = details.First().TimeSlot;
            ViewData["UserEmail"] = user.Email;
            return View(details);
        }

    }
}
