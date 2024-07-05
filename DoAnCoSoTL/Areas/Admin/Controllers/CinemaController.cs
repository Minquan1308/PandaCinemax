
using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;

namespace DoAnCoSoTL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CinemaController : Controller
    {
        private readonly ICinemaRepository _cinemaRepository;
        MovieContext _db;
        private readonly IMovieRepository _movieRepository;
        public CinemaController(ICinemaRepository cinemaRepository, MovieContext db, IMovieRepository movieRepository)
        {
            _cinemaRepository = cinemaRepository;
            _db = db;
            _movieRepository = movieRepository;
        }
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName); // Thay  đổi đường dẫn theo cấu hình của bạn 
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối }

        }
        public async Task<IActionResult> RoomIndex()
        {
            var rooms = await _db.Rooms
                        .Include(r => r.Cinema)
                        .ToListAsync();
            return View(rooms);
        }
        public async Task<IActionResult> RoomDetails(int id)
        {
            var rooms = await _db.Rooms.FindAsync(id);
            return View("RoomDetails", rooms);
        }
        [HttpPost]
        public async Task<IActionResult> RoomDelete(int id)
        {
            var order = _db.SeatOrderDetails.Where(s => s.RoomId == id).ToList();

            foreach (var item in order)
            {
                if (item.SeatOrder.OrderDate >= DateTime.Now.Date)
                {
                    return Json(new { success = false, message = "This screening room has been booked and cannot be deleted " });
                }
            }
          
            var room = await _db.Rooms.FindAsync(id);
            if (room != null)
            {
                _db.Rooms.Remove(room);
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Không tìm thấy phòng" });
        }




        
        public async Task<IActionResult> AddRoom()
        {
            ViewBag.Cinemas = new SelectList(_db.Cinemas.ToList(), "Id", "Name");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddRoom(Room m)
        {
            if (!ModelState.IsValid)
            {
                if (m.soHang >= 4 && m.soCot >= 5)
                {
                    _db.Rooms.Add(m);
                    await _db.SaveChangesAsync(); // Sử dụng SaveChangesAsync để đảm bảo Room được lưu trước
                }
                // Chuyển hướng về trang Index của Cinema
                return RedirectToAction("RoomIndex", "Cinema");
            }

            // Nếu model không hợp lệ, trả lại view với lỗi
            return View(m);
        }
        [HttpPost]
        public IActionResult GetSelectedRooms(int cinemaId)
        {
            var rooms = _db.Rooms.Where(r => r.CinemaId == cinemaId).Select(r => new { r.Id }).ToList();
            return Json(rooms);
        }

        public async Task<IActionResult> Index()
        {
            var cinemas = await _cinemaRepository.GetAllAsync();
            return View(cinemas);
        }
        
        public async Task<IActionResult> Details(int id)
        {

            Cinema cinemas = await _cinemaRepository.GetByIdAsync(id);
            return View("Details", cinemas);
        }
        public IActionResult Create()
        {
            return View("Create", new Cinema());
        }

        //action xử lý thêm category
        [HttpPost]
        public async Task<IActionResult> Create(Cinema newCinema, IFormFile Image)
        {
            var cinemas = await _cinemaRepository.GetAllAsync();
            foreach (var item in cinemas)
            {
                if (item.Name == newCinema.Name)
                {
                    ModelState.AddModelError("Name", "This cinema name already exists.");
                }
            }
            if (ModelState.IsValid)
            {
                    
                if (Image != null)
                {
                    // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                    newCinema.Image = await SaveImage(Image);
                }
                await _cinemaRepository.InsertAsync(newCinema);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View("Create", newCinema);
            }
            
        }
       
        // Xử lý cập nhật sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromForm] Cinema cinema, IFormFile Image)
        {
            ModelState.Remove("Image");
            var existingCinema = await _cinemaRepository.GetByIdAsync(id);
            if (existingCinema == null)
            {
                return NotFound();
            }

            var cinemas = await _cinemaRepository.GetAllAsync();
            foreach (var item in cinemas)
            {
                if (item.Id != id && item.Name == cinema.Name)
                {
                    ModelState.AddModelError("Name", "This cinema name already exists.");
                    break; // Dừng vòng lặp khi tìm thấy rạp chiếu trùng
                }
            }
            if (!ModelState.IsValid)
            {
                return View(cinema);
            }
            else
            {
                if (Image != null)
                {
                    // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                    existingCinema.Image = await SaveImage(Image);
                }
                // Nếu không có lỗi, cập nhật thông tin của rạp chiếu và lưu vào cơ sở dữ liệu
                existingCinema.Name = cinema.Name;
                existingCinema.Description = cinema.Description; // Hoặc bất kỳ trường dữ liệu nào khác bạn muốn cập nhật

                await _cinemaRepository.UpdateAsync(existingCinema);
                return RedirectToAction(nameof(Index));
            }
        }

        //CinemaController:
        public async Task<IActionResult> Update(int id)
        {
            var cinema = await _cinemaRepository.GetByIdAsync(id);
            var futureScreenings = _db.Screenings
        .Where(s => s.CinemaId == id && s.Date >= DateTime.Now)
        .Select(s => s.Id)
        .ToList();

            var bookedSeats = _db.SeatOrderDetails
                .Where(s => futureScreenings.Contains(s.ScreeningId))
                .ToList();

            if (bookedSeats.Any())
            {
                TempData["ErrorMessage"] = "This cinema has been set and cannot be edited";
                return RedirectToAction("Index", "Cinema");
            }
            if (cinema == null)
            {
                return NotFound();
            }

            if (cinema.Image != null)
            {
                cinema.Image = "default.jpg"; // Set a default image path or handle accordingly
            }

            return View(cinema);
        }

        // Xử lý cập nhật sản phẩm
        
        // Xử lý xóa actor
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _cinemaRepository.GetByIdAsync(id);
            var futureScreenings = _db.Screenings
            .Where(s => s.CinemaId == id && s.Date >= DateTime.Now)
            .Select(s => s.Id)
            .ToList();

            var bookedSeats = _db.SeatOrderDetails
                .Where(s => futureScreenings.Contains(s.ScreeningId))
                .ToList();

            if (bookedSeats.Any())
            {
                return Json(new { success = false, message = "This cinema has been set and cannot be deleted" });
            }

            await _cinemaRepository.DeleteAsync(id);
          
            return Json(new { success = true, message = "Successfully deleted the cinema" });
        }
        [HttpGet]
        public async Task<IActionResult> Index(string Keyword)
        {
            ViewData["searching"] = Keyword;
            var cinemas = _db.Cinemas.Select(x => x);
            if (!string.IsNullOrEmpty(Keyword))
            {
                cinemas = cinemas.Where(c => c.Name.Contains(Keyword) || c.Location.Contains(Keyword));

            }
            return View(await cinemas.AsNoTracking().ToListAsync());
        }
    }
}
