using DoAnCoSoTL.Extensions;
using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using DoAnCoSoTL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Controllers
{
    [Authorize]
    public class SeatBookingCartController : Controller
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IVnPayService _vnPayService;
        private readonly MovieContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public SeatBookingCartController(MovieContext context,
        UserManager<ApplicationUser> userManager, IMovieRepository
        movieRepository, ISeatRepository seatRepository, IVnPayService vnPayService)
        {
            _movieRepository = movieRepository;
            _context = context;
            _userManager = userManager;
            _seatRepository = seatRepository;
            _vnPayService = vnPayService;
        }

        public async Task<IActionResult> AddToCart(int seatId)
        {
            //seatId = 410;
            var seat = await GetSeatFromDatabase(seatId);

            if (seat == null || seat.Screening == null || seat.Screening.Movie == null)
            {
                TempData["ErrorMessage"] = "Không thể thêm vé vào giỏ hàng. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
            var room = _context.Rooms.SingleOrDefault(s => s.Id == seat.RoomId);
            var cinema = _context.Cinemas.SingleOrDefault(s => s.Id == room.CinemaId);

            var cartItem = new SeatCartItem
            {
                ScreeningId = seat.ScreeningId,
                SeatId = seatId,
                MovieId = seat.Screening.Movie.Id,
                MovieName = seat.Screening.Movie.Name,
                CinemaName = cinema.Name,
                CinemaLocation = cinema.Location,
                TimeSlot = $"{seat.Screening.Time} - {seat.Screening.EndTime}",
                SeatCode = seat.SeatCode,
                Price = (decimal)seat.Screening.Movie.Price,
                RoomId = room.Id

            };

            var cart = HttpContext.Session.GetObjectFromJson<SeatBookingCart>("Cart") ?? new SeatBookingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index(int roomId, int screeningId, string timeSlot, Guid movieId)
        {
            // Lấy thông tin từ cart
            var cart = HttpContext.Session.GetObjectFromJson<SeatBookingCart>("Cart") ?? new SeatBookingCart();

            // Lấy thông tin về phương thức thanh toán
            var user = await _userManager.GetUserAsync(User);
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            List<SelectListItem> paymentMethods;

            if (isCustomer)
            {
                paymentMethods = new List<SelectListItem>
        {
            new SelectListItem { Value = "VNPay", Text = "Thanh toán VNPay" }
        };
            }
            else
            {
                paymentMethods = new List<SelectListItem>
        {
            new SelectListItem { Value = "COD", Text = "Thanh toán Cash" },
            new SelectListItem { Value = "VNPay", Text = "Thanh toán VNPay" }
        };
            }

            ViewBag.PaymentMethods = paymentMethods;

            // Chuyển toàn bộ thông tin cần thiết tới view
            ViewData["RoomId"] = roomId;
            ViewData["ScreeningId"] = screeningId;
            ViewData["TimeSlot"] = timeSlot;
            ViewData["MovieId"] = movieId;
            ViewData["Cart"] = cart;

            return View(cart);
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(SeatOrder order, string method)
        {
            var cart = HttpContext.Session.GetObjectFromJson<SeatBookingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống. Hãy thêm vé xem phim vào giỏ hàng trước khi tiếp tục.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);

            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.PaymentMethods = method == "VNPay" ? "VNPay" : "COD";

            order.SeatOrderDetails = cart.Items.Select(i => new SeatOrderDetail
            {
                ScreeningId = i.ScreeningId,
                SeatId = i.SeatId,
                MovieId = i.MovieId,
                Quantity = 1,
                CinemaName = i.CinemaName,
                TimeSlot = i.TimeSlot,
                CinemaLocation = i.CinemaLocation,
                SeatCode = i.SeatCode,
                Price = i.Price,
                RoomId = i.RoomId
            }).ToList();

            _context.SeatOrders.Add(order);

            foreach (var item in cart.Items)
            {
                var movie = await _context.Movies.FindAsync(item.MovieId);
                if (movie != null)
                {
                    movie.TotalQuantitySold += item.Quantity;
                    _context.Movies.Update(movie);
                    order.TotalQuantitySold += item.Quantity;
                }
            }

            var seatIds = cart.Items.Select(i => i.SeatId).ToList();
            var seatsToUpdate = _context.Seats.Where(s => seatIds.Contains(s.Id)).ToList();
            foreach (var seat in seatsToUpdate)
            {
                seat.IsAvailable = false;
            }

            await _context.SaveChangesAsync();

            if (method == "VNPay")
            {
                var vnPayModel = new VnPaymentRequestModel
                {
                    Amount = (double)order.SeatOrderDetails.Sum(p => p.Price * p.Quantity),
                    CreatedDate = DateTime.Now,
                    Description = "Thanh toán đơn hàng",
                    FullName = order.ApplicationUser.FullName,
                    OrderId = new Random().Next(1000, 100000)
                };

                // Save orderId to session as string
                HttpContext.Session.SetString("OrderId", order.Id.ToString());

                // Get the VNPay URL
                var paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, vnPayModel);

                // Redirect directly to the VNPay URL
                return Redirect(paymentUrl);
            }

            HttpContext.Session.Remove("Cart");
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            if (isCustomer)
            {
                return RedirectToAction("Detail", "OrderList", new { orderId = order.Id });
            }
            TempData["Message"] = "Thanh toán bằng tiền mặt thành công.";
            return RedirectToAction("Detail", "OrderList", new { orderId = order.Id });
        }



        private async Task<Seat> GetSeatFromDatabase(int seatId)
        {
            var seat = await _context.Seats
        .Include(s => s.Screening)
            .ThenInclude(sc => sc.Movie)
        .FirstOrDefaultAsync(s => s.Id == seatId);
            //var seat = await _seatRepository.GetByIdAsync(seatId);
            if (seat != null && seat.Screening != null && seat.Screening.Movie != null)
            {
                return seat;
            }
            return null;
        }

        public IActionResult RemoveFromCart(int seatId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<SeatBookingCart>("Cart");
            if (cart is not null)
            {
                cart.RemoveItem(seatId);

                // Lưu lại giỏ hàng vào Session sau khi đã xóa mục
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> ClearCart(Guid movieId)
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index", "Screening", new { movieId = movieId, datetime = DateTime.Today.Date });
        }
        public IActionResult OrderCompleted(int orderId)
        {
            // Xử lý đơn hàng đã hoàn thành và trả về view
            return View(orderId);
        }
        //public IActionResult PaymentFail()
        //{
        //    return View();
        //}
        //public IActionResult PaymentSuccess()
        //{
        //    return View();
        //}
        [Authorize]
        public IActionResult PaymentCallback(int roomId, int screeningId, string timeSlot, Guid movieId)
        {
            var response = _vnPayService.PaymentExcute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay : {response.VnPayResponseCode}";

                // Nếu thanh toán bị hủy, cập nhật trạng thái ghế và giữ nguyên giỏ hàng
                var orderIdString1 = HttpContext.Session.GetString("OrderId");
                if (Guid.TryParse(orderIdString1, out Guid orderId1))
                {
                    var order = _context.SeatOrders
                        .Include(o => o.SeatOrderDetails)
                        .FirstOrDefault(o => o.Id == orderId1);

                    if (order != null)
                    {
                        // Đặt lại trạng thái ghế
                        var seatIds = order.SeatOrderDetails.Select(d => d.SeatId).ToList();
                        var seatsToUpdate = _context.Seats.Where(s => seatIds.Contains(s.Id)).ToList();
                        foreach (var seat in seatsToUpdate)
                        {
                            seat.IsAvailable = true;
                        }
                        // Lưu thay đổi vào cơ sở dữ liệu
                        _context.SaveChanges();
                    }

                    // Lấy thông tin từ SeatOrderDetail trong order để truyền vào các biến
                    roomId = order.SeatOrderDetails.FirstOrDefault()?.RoomId ?? roomId;
                    screeningId = order.SeatOrderDetails.FirstOrDefault()?.ScreeningId ?? screeningId;
                    timeSlot = order.SeatOrderDetails.FirstOrDefault()?.TimeSlot ?? timeSlot;
                    movieId = order.SeatOrderDetails.FirstOrDefault()?.MovieId ?? movieId;

                    // Xóa đơn hàng đã tạo ra trước đó
                    _context.SeatOrders.Remove(order);
                    _context.SaveChanges();

                    // Xóa OrderId khỏi session
                    HttpContext.Session.Remove("OrderId");
                }

                // Chuyển hướng về trang Index của SeatBookingCartController
                return RedirectToAction("Index", "SeatBookingCart", new { roomId = roomId, screeningId = screeningId, timeSlot = timeSlot, movieId = movieId });
            }

            HttpContext.Session.Remove("Cart");
            TempData["Message"] = $"Thanh toán VNPay thành công.";

            // Retrieve orderId from session and convert to Guid
            var orderIdString = HttpContext.Session.GetString("OrderId");
            if (Guid.TryParse(orderIdString, out Guid orderId))
            {
                HttpContext.Session.Remove("OrderId");
                return RedirectToAction("Detail", "OrderList", new { orderId });
            }

            return RedirectToAction("Index", "OrderList");
        }


    }
}