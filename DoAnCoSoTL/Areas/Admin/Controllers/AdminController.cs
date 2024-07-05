using DoAnCoSoTL.Migrations;
using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SQLitePCL;
namespace DoAnCoSoTL.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly MovieContext _context;
        private readonly IMovieRepository _movieRepository;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, MovieContext context, IMovieRepository movieRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _movieRepository = movieRepository;
        }
        public async Task<IActionResult> CreateAdminAccount()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var user = new ApplicationUser
            {
                FullName = "Thu Phương",
                UserName = "Admin1@gmail.com",
                Email = "Admin1@gmail.com"
            };

            var result = await _userManager.CreateAsync(user, "Abc@123");

            if (result.Succeeded)
            {
                // Kiểm tra người dùng đã được tạo thành công
                var createdUser = await _userManager.FindByEmailAsync("Admin1@gmail.com");
                if (createdUser != null)
                {
                    await _userManager.AddToRoleAsync(createdUser, "Admin");
                    return Content("Admin Account Created Successfully!");
                }
                else
                {
                    return BadRequest("Failed to Create Admin Account: User not found");
                }
            }
            else
            {
                return BadRequest("Failed to Create Admin Account: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        public async Task<IActionResult> IndexAsync()
        {
            var users = _userManager.Users.ToList(); // Lấy danh sách tất cả người dùng

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.Role = roles.FirstOrDefault();
            }
            return View(users);
        }
        public async Task<IActionResult> Delete(String employeeId)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(employeeId);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> DailyRevenue()
        {
            List<SelectListItem> cinemas = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "All" }
            };
            cinemas.AddRange(new SelectList(_context.Cinemas.ToList(), "Id", "Name"));
            var date = DateTime.Today.Date;

            // Truyền tên của chi nhánh đầu tiên để tìm kiếm
            var firstCinema = cinemas.FirstOrDefault()?.Text ?? "All";

            return await DailyRevenue(date, firstCinema);
        }
        [HttpPost]
        public async Task<IActionResult> DailyRevenue(DateTime? date, string location)
        {
            if (date == null)
            {
                date = DateTime.Today.Date; //Nếu date null thì set date bằng ngày hiện tại
            }

            ViewBag.SelectedCinema = location; //gắn rạp được chọn bằng location 
            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");
            //DateTime startOfMonth = new DateTime(date.Value.Year, date.Value.Month, 1);
            var startOfMonth = new DateTime(date.Value.Year, date.Value.Month, 1); //Ngày đầu tháng của date
            DateTime endOfMonth;
            if (date.Value.Month == DateTime.Now.Month && date.Value.Year == DateTime.Now.Year)
            {
                endOfMonth = DateTime.Now;
            }
            else
            {
                endOfMonth = startOfMonth.AddMonths(1).AddDays(-1); //gán ngày cuối tháng
            }
            Decimal totalMonthlyRevenue;
            List<SeatOrderDetail> orderDetails;
            Decimal monthlyRevenueUpToSelectedDate;
            List<SeatOrder> seatOrders;
            if (location == "All") //nếu location bằng All thì truy vấn order có ngày bằng ngày hiện tại được chọn
            {
                seatOrders = _context.SeatOrders
                                      .Where(order => order.OrderDate.Date == date)
                                      .Include(order => order.SeatOrderDetails)  //bao gồm thông tin về SeatOrderDetail
                                      .ToList();

                
                orderDetails = seatOrders
                    .SelectMany(order => order.SeatOrderDetails)
                    .ToList();

                monthlyRevenueUpToSelectedDate = _context.SeatOrderDetails
                .Where(detail => detail.SeatOrder.OrderDate.Date >= startOfMonth && detail.SeatOrder.OrderDate.Date <= date)
                .Sum(i => i.Quantity * i.Price); //doanh thu từ đầu tháng tính đến date đang được chọn
                totalMonthlyRevenue = _context.SeatOrderDetails
                .Where(detail => detail.SeatOrder.OrderDate.Date >= startOfMonth && detail.SeatOrder.OrderDate.Date <= endOfMonth)
                .Sum(i => i.Quantity * i.Price); //doanh thu cả tháng
            }
            else
            {
                seatOrders = _context.SeatOrders
                              .Where(order => order.OrderDate.Date == date)
                              .Where(order => order.SeatOrderDetails.Any(detail => detail.CinemaName == location))  //truy vấn list SeatOrder khi có ngày bằng date và tên rạp trong detail bằng location
                              .Include(order => order.SeatOrderDetails)  
                              .ToList();

                
                orderDetails = seatOrders
                    .SelectMany(order => order.SeatOrderDetails)
                    .ToList();


                monthlyRevenueUpToSelectedDate = _context.SeatOrderDetails
                .Where(detail => detail.SeatOrder.OrderDate.Date >= startOfMonth && detail.SeatOrder.OrderDate.Date <= date && detail.CinemaName == location)
                .Sum(i => i.Quantity * i.Price); // doanh thu kể từ ngày đầu tháng đến date được chọn thuộc rạp có tên là location
                totalMonthlyRevenue = _context.SeatOrderDetails
                .Where(detail => detail.SeatOrder.OrderDate.Date >= startOfMonth && detail.SeatOrder.OrderDate.Date <= endOfMonth && detail.CinemaName == location)
                .Sum(i => i.Quantity * i.Price); // doanh thu cả tháng của rạp có tên là location
            }
            decimal Total = orderDetails.Sum(i => i.Quantity * i.Price); // tính doanh thu ngày hiện tại
            decimal COD = orderDetails.Where(detail => detail.SeatOrder.PaymentMethods == "COD").Sum(i => i.Quantity * i.Price); //tổng tiền thanh toán bằng phương thức COD
            decimal VNPay = orderDetails.Where(detail => detail.SeatOrder.PaymentMethods == "VNPay").Sum(i => i.Quantity * i.Price);//tổng tiền thanh toán bằng phương thức VNPay
            decimal MonthlyRevenueUpToSelectedDate = monthlyRevenueUpToSelectedDate;
            ViewBag.Total = Total;
            ViewBag.COD = COD;
            ViewBag.VNPay = VNPay;
            ViewBag.MonthlyRevenueUpToSelectedDate = MonthlyRevenueUpToSelectedDate;
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



    }
}

