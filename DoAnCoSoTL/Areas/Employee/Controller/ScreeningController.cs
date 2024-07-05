
using DoAnCoSoTL.Extensions;
using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Net.WebSockets;

namespace DoAnCoSoTL.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = SD.Role_Employee)]
    public class ScreeningController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MovieContext _context;
        private readonly ICinemaRepository _cinemaRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IOrderRepository _orderRepository;
        public ScreeningController(MovieContext context, ICinemaRepository cinemaRepository, IMovieRepository movieRepository, UserManager<ApplicationUser> userManager, IOrderRepository orderRepository)
        {
            _context = context;
            _cinemaRepository = cinemaRepository;
            _movieRepository = movieRepository;
            _userManager = userManager;
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index(DateTime? date)
        {
            if (date == null)
            {
                date = DateTime.Today.Date; // Nếu không có ngày nào được chọn, mặc định là ngày hiện tại
            }

            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd"); // Lưu trữ ngày đã chọn trong ViewBag

            var screenings = _context.Screenings.Where(s => s.Date.Date == date).ToList();
            foreach (var screening in screenings)
            {
                screening.Movie = await _movieRepository.GetByIdAsync(screening.MovieId);
                screening.Cinema = await _cinemaRepository.GetByIdAsync(screening.CinemaId);
            }

            return View("Index", screenings);
        }

        public async Task<IActionResult> DailyRevenue(DateTime? date)
        {
            var user = await _userManager.GetUserAsync(User);
            if (date == null)
            {
                date = DateTime.Now.Date;
            }
            string location = user.Address;

            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");

            var seatOrders = _context.SeatOrders
                                     .Where(order => order.OrderDate.Date == date)
                                     .Where(order => order.ApplicationUser.Address == location)
                                     .Include(order => order.SeatOrderDetails)  // Include SeatOrderDetails to ensure they are loaded
                                     .ToList();

            // Aggregate the order details from all seat orders
            var orderDetails = seatOrders
                .SelectMany(order => order.SeatOrderDetails)
                .ToList();

            decimal Total = orderDetails.Sum(i => i.Quantity * i.Price);
            decimal COD = orderDetails.Where(detail => detail.SeatOrder.PaymentMethods == "COD").Sum(i => i.Quantity * i.Price);
            decimal VNPay = orderDetails.Where(detail => detail.SeatOrder.PaymentMethods == "VNPay").Sum(i => i.Quantity * i.Price);

            var startOfMonth = new DateTime(date.Value.Year, date.Value.Month, 1);
            var monthlyRevenueUpToSelectedDate = _context.SeatOrderDetails
                .Where(detail => detail.SeatOrder.OrderDate.Date >= startOfMonth && detail.SeatOrder.OrderDate.Date <= date && detail.CinemaName == location)
                .Sum(i => i.Quantity * i.Price);

            DateTime endOfMonth;
            if (date.Value.Month == DateTime.Now.Month && date.Value.Year == DateTime.Now.Year)
            {
                endOfMonth = DateTime.Now;
            }
            else
            {
                endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            }
            var totalMonthlyRevenue = _context.SeatOrderDetails
                .Where(detail => detail.SeatOrder.OrderDate.Date >= startOfMonth && detail.SeatOrder.OrderDate.Date <= endOfMonth && detail.CinemaName == location)
                .Sum(i => i.Quantity * i.Price);

            ViewBag.Total = Total;
            ViewBag.COD = COD;
            ViewBag.VNPay = VNPay;
            ViewBag.MonthlyRevenueUpToSelectedDate = monthlyRevenueUpToSelectedDate;
            ViewBag.TotalMonthlyRevenue = totalMonthlyRevenue;

            var detailsWithMovieInfo = seatOrders
                .Select(order => new DetailWithMovieInfoModel
                {
                    OrderId = order.Id,
                    OrderDate = order.OrderDate,
                    UserId = order.UserId,
                    Price = order.TotalPrice,
                    MovieName = string.Join(", ", order.SeatOrderDetails.Select(d => _context.Movies.FirstOrDefault(movie => movie.Id == d.MovieId)?.Name ?? "Unknown").Distinct()),
                    DetailId = order.SeatOrderDetails.FirstOrDefault()?.Id ?? Guid.Empty // Assume the first detail Id for demonstration
                }).ToList();

            return View(detailsWithMovieInfo);
        }
        public async Task<IActionResult> Details(Guid orderId)
        {

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
            return View(details);
        }
        public IActionResult ViewSeat(int roomId, int screeningId, string timeSlot, Guid movieId)
        {
            if (screeningId == 0 || roomId == 0 || movieId == Guid.Empty)
            {
                // Log hoặc xử lý lỗi ở đây
                return BadRequest("Invalid parameters.");
            }
            var room = _context.Rooms.FirstOrDefault(x => x.Id == roomId);
            ViewBag.Row = room.soHang;
            ViewBag.Number = room.soCot;

            var seats = GenerateSeatsForScreening(screeningId, room.soHang, room.soCot, roomId);

            var cart = HttpContext.Session.GetObjectFromJson<SeatBookingCart>("Cart") ?? new SeatBookingCart();
            var selectedSeatIds = cart.Items.Select(item => item.SeatId).ToList();
            var movie = _context.Movies.SingleOrDefault(x => x.Id == movieId);

            ViewData["RoomId"] = roomId;
            ViewData["ScreeningId"] = screeningId;
            ViewData["TimeSlot"] = timeSlot;
            ViewData["MovieName"] = movie.Name;
            ViewData["MovieId"] = movieId;
            ViewData["SelectedSeatIds"] = selectedSeatIds;
            ViewBag.TicketPrice = movie.Price;
            return View(seats); // Pass the seats list to the view
        }
        private List<Seat> GenerateSeatsForScreening(int screeningId, int hang, int cot, int roomId)
        {
            // Lấy thông tin về khung giờ chiếu để tạo các ghế
            var screening = _context.Screenings
                .Include(s => s.Room)
                .FirstOrDefault(s => s.Id == screeningId);

            if (screening == null)
            {
                return new List<Seat>(); // Trả về danh sách trống nếu không tìm thấy khung giờ chiếu
            }

            // Kiểm tra xem đã tạo ghế cho khung giờ chiếu này trước đó chưa
            var existingSeats = _context.Seats.Any(s => s.ScreeningId == screeningId);

            // Nếu đã tồn tại ghế cho khung giờ chiếu này, chỉ cần trả về danh sách các ghế từ cơ sở dữ liệu
            if (existingSeats)
            {
                var seatsFromDatabase = _context.Seats.Where(s => s.ScreeningId == screeningId).ToList();
                return seatsFromDatabase;
            }
            // Nếu chưa có ghế được tạo cho khung giờ chiếu này, tạo mới danh sách ghế và lưu vào cơ sở dữ liệu
            var seats = new List<Seat>();
            for (int row = 1; row <= hang; row++)
            {
                for (int seatNumber = 1; seatNumber <= cot; seatNumber++)
                {
                    // Tạo mã ghế
                    string seatCode = $"{(char)('A' + row - 1)}{seatNumber}";

                    // Tạo đối tượng ghế mới và thêm vào danh sách
                    var seat = new Seat
                    {
                        SeatCode = seatCode,
                        Row = row.ToString(),
                        Number = seatNumber,
                        IsAvailable = true, // Mặc định là ghế trống
                        //CinemaName = screening.Room.Cinema.Name,
                        ScreeningId = screeningId,
                        RoomId = roomId

                    };

                    seats.Add(seat);
                }
            }

            // Lưu danh sách các ghế vào cơ sở dữ liệu
            _context.Seats.AddRange(seats);
            _context.SaveChanges();

            return seats;
        }



        public async Task<IActionResult> TicketSales()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var cinema = await _cinemaRepository.GetByNameAsync(user.Address);
            if (cinema == null)
            {
                return NotFound();
            }

            //var screenings = await _context.Screenings
            //    .Where(s => s.CinemaId == cinema.Id && s.Date.Date == DateTime.Today)
            //    .ToListAsync();
            var screenings = await _context.Screenings
            .Where(s => s.CinemaId == cinema.Id && s.Date.Date == DateTime.Today)
            .Include(s => s.Movie)
            .ThenInclude(m => m.Category) // Include Category information
            .ToListAsync();
            var currentTime = DateTime.Now.TimeOfDay; // Lấy giờ phút hiện tại
            var filteredScreenings = new List<Screening>();
            foreach (var screening in screenings)
            {
                screening.Movie = await _movieRepository.GetByIdAsync(screening.MovieId);
                TimeSpan endTime;
                if (TimeSpan.TryParse(screening.EndTime, out endTime)) // assuming EndTime is a string property
                {
                    if(endTime > currentTime) 
                    {
                        filteredScreenings.Add(screening);
                        
                    }

                   
                }
            }

            ViewBag.Cinema = user.Address;
            return View("TicketSales", filteredScreenings); // Truyền screenings vào view
        }


    }
}