using DoAnCoSoTL.Migrations;
using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DoAnCoSoTL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ScreeningController : Controller
    {
        private readonly IScreeningRepository _screeningRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly ICinemaRepository _cinemaRepository;
        private readonly IMovieInCinemaRepository _cinemainCinemaRepository;
        private readonly MovieContext _db;

        public ScreeningController(IScreeningRepository screeningRepository, IMovieRepository movieRepository, ICinemaRepository cinemaRepository, MovieContext db, IMovieInCinemaRepository cinemainCinemaRepository)
        {
            _screeningRepository = screeningRepository;
            _movieRepository = movieRepository;
            _cinemaRepository = cinemaRepository;
            _db = db;
            _cinemainCinemaRepository = cinemainCinemaRepository;
        }

        public async Task<IActionResult> Index()
        {
            var screenings = await _screeningRepository.GetAllAsync();


            // Thực hiện eager loading cho thuộc tính Cinema
            foreach (var screening in screenings)
            {
                await _db.Entry(screening)
                    .Reference(s => s.Cinema)
                    .LoadAsync();
            }

            // Thực hiện eager loading cho thuộc tính Movie
            foreach (var screening in screenings)
            {
                await _db.Entry(screening)
                    .Reference(s => s.Movie)
                    .LoadAsync();
            }

            return View(screenings);
        }




        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var screening = new Screening();

            // Fetch cinemas and movies
            var cinemas = await _cinemaRepository.GetAllAsync();
            var list = _db.Movies.Where(x => x.EndDate >= DateTime.Today).ToList();

            if (list.Any())
            {
                var movieDurationMinutes = list.First().DurationMinutes;
                ViewBag.MovieDurationMinutes = movieDurationMinutes;
            }

            var defaultCinemaId = cinemas.FirstOrDefault()?.Id ?? 0;
            var rooms = _db.Rooms.Where(x => x.CinemaId == defaultCinemaId).ToList();

            ViewBag.Cinemas = new SelectList(cinemas, "Id", "Name");
            ViewBag.Movies = new SelectList(list, "Id", "Name");
            //ViewBag.Rooms = new SelectList(rooms, "Id", "Id");

            return View(screening);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Screening screening)
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    // Lấy ngày bắt đầu và kết thúc của Movie
                    var list = _db.Movies.Where(x => x.EndDate >= DateTime.Today).ToList();
                      
                    
                    var movie = await _movieRepository.GetByIdAsync(screening.MovieId);
                    if (movie == null)
                    {
                        // Xử lý trường hợp movie không tồn tại
                        ModelState.AddModelError(string.Empty, "Cound not found the movie");
                        return View(screening);
                    }
                    
                    else
                    {
                        // Kiểm tra ngày hợp lệ khi movie tồn tại
                        if (screening.Date < movie.StartDate || screening.Date > movie.EndDate)
                        {
                            ModelState.AddModelError(string.Empty, "Start date is not valid");

                            ViewBag.Movies = new SelectList(list, "Id", "Name");
                            return View(screening);
                        }
                    }

                    screening.Movie = movie;
                    var screeninglist = await _screeningRepository.GetAllAsync();

                    // Lấy danh sách lịch chiếu trong cùng phòng chiếu và sắp xếp theo thời gian giảm dần
                    var roomScreenings = screeninglist
                        .Where(s => s.CinemaId == screening.CinemaId && s.Date == screening.Date && s.RoomId == screening.RoomId)
                        .OrderByDescending(s => s.Time)
                        .ToList();

                    foreach (var item in screeninglist)
                    {
                        if (item.CinemaId == screening.CinemaId && item.Date == screening.Date)
                        {
                            if (item.Time == screening.Time && item.RoomId != screening.RoomId && item.MovieId == screening.MovieId)
                            {
                                string errorMessage = $"Lịch chiếu vào lúc {item.Time} cho bộ phim {item.Movie.Name} đã được thêm vào phòng chiếu {item.RoomId}";
                                ModelState.AddModelError(string.Empty, errorMessage);

                                ViewBag.Movies = new SelectList(list, "Id", "Name");
                                return View(screening);
                            }
                            if (item.Time == screening.Time && item.RoomId == screening.RoomId)
                            {
                                string errorMessage = $"Lịch chiếu vào lúc {item.Time} tại phòng chiếu {item.RoomId} đã tồn tại";
                                ModelState.AddModelError(string.Empty, errorMessage);

                                ViewBag.Movies = new SelectList(list, "Id", "Name");
                                return View(screening);
                            }
                        }
                    }

                    // Kiểm tra các lịch chiếu trong cùng phòng chiếu
                    if (roomScreenings.Any())
                    {
                        var latestScreening = roomScreenings.First();
                        TimeSpan latestEndTime = TimeSpan.Parse(latestScreening.EndTime);
                        TimeSpan newScreeningStartTime = TimeSpan.Parse(screening.Time);
                        TimeSpan difference = newScreeningStartTime - latestEndTime;

                        if (latestEndTime >= newScreeningStartTime || difference.TotalMinutes < 30 || difference.TotalMinutes > 60)
                        {
                            string errorMessage = $"Các lịch chiếu cùng phòng phải cách nhau ít nhất 30 phút và không quá 60 phút : {latestScreening.EndTime}";
                            ModelState.AddModelError(string.Empty, errorMessage);

                            ViewBag.Movies = new SelectList(list, "Id", "Name");
                            return View(screening);
                        }
                    }

                    // Lưu đối tượng Screening vào cơ sở dữ liệu
                    await _screeningRepository.InsertScreeningAsync(screening);
                    return RedirectToAction("Index", "Screening");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while saving data: " + ex.Message);
                }
            }

            // Lấy danh sách rạp chiếu và danh sách phim từ cơ sở dữ liệu
            var cinemas = await _cinemaRepository.GetAllAsync();
            var movies = await _movieRepository.GetAllAsync();

            // Trả về view với đối tượng Screening và danh sách rạp chiếu, danh sách phim
            ViewBag.Cinemas = new SelectList(cinemas, "Id", "Name");
            ViewBag.Movies = new SelectList(movies, "Id", "Name");

            return View(screening);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var screening = await _screeningRepository.GetByIdAsync(id);
            if (screening == null)
            {
                return NotFound();
            }    

            var order = _db.SeatOrderDetails.Where(s => s.ScreeningId == screening.Id).ToList();

            //foreach (var item in order)
            //{
            //    item.SeatOrder = _db.SeatOrders.FirstOrDefault(s=>s.Id==item.SeatOrderId);

            if ( order.Any())
            {
                TempData["ErrorMessage"] = "This showtime has been booked and cannot be edited";
                return RedirectToAction("Index");
            }
          
            // Lấy thông tin của buổi chiếu từ cơ sở dữ liệu
           

            // Lấy danh sách rạp chiếu và danh sách phim từ cơ sở dữ liệu
            var cinemas = await _cinemaRepository.GetAllAsync();
            var list = _db.Movies.Where(x => x.EndDate >= DateTime.Today).ToList();

            // Trả về view với thông tin của buổi chiếu và danh sách rạp chiếu, danh sách phim
            ViewBag.Cinemas = new SelectList(cinemas, "Id", "Name", screening.CinemaId);
            ViewBag.Movies = new SelectList(list, "Id", "Name", screening.MovieId);

            return View(screening);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Screening screening)
        {

            if (id != screening.Id)
            {
                return NotFound();
            }
            
            if (!ModelState.IsValid)
            {
                try
                {
                    // Lấy đối tượng Screening hiện tại từ cơ sở dữ liệu
                    var existingScreening = await _screeningRepository.GetByIdAsync(id);

                    if (existingScreening == null)
                    {
                        return NotFound();
                    }
                    var list = _db.Movies.Where(x => x.EndDate >= DateTime.Today).ToList();
                    var movie = await _movieRepository.GetByIdAsync(screening.MovieId);
                    if (movie == null)
                    {
                        // Xử lý trường hợp movie không tồn tại
                        ModelState.AddModelError(string.Empty, "Không tìm thấy thông tin phim");
                        return View(screening);
                    }
                    else
                    {
                        // Kiểm tra ngày hợp lệ khi movie tồn tại
                        if (screening.Date < movie.StartDate || screening.Date > movie.EndDate)
                        {
                            ModelState.AddModelError(string.Empty, "Ngày chiếu phim không hợp lệ");

                            ViewBag.Movies = new SelectList(list, "Id", "Name");
                            return View(screening);
                        }
                    }

                    screening.Movie = movie;
                    var screeninglist = await _screeningRepository.GetAllAsync();

                    // Lấy danh sách lịch chiếu trong cùng phòng chiếu và sắp xếp theo thời gian giảm dần
                    var roomScreenings = screeninglist
                        .Where(s => s.CinemaId == screening.CinemaId && s.Date == screening.Date && s.RoomId == screening.RoomId)
                        .OrderByDescending(s => s.Time)
                        .ToList();

                    foreach (var item in screeninglist)
                    {
                        if (item.Id != screening.Id)
                        {
                            if (item.CinemaId == screening.CinemaId && item.Date == screening.Date)
                            {
                                if (item.Time == screening.Time && item.RoomId != screening.RoomId && item.MovieId == screening.MovieId)
                                {
                                    string errorMessage = $"Lịch chiếu vào lúc {item.Time} cho bộ phim {item.Movie.Name} đã được thêm vào phòng chiếu {item.RoomId}";
                                    ModelState.AddModelError(string.Empty, errorMessage);

                                    ViewBag.Movies = new SelectList(list, "Id", "Name");
                                    return View(screening);
                                }
                                if (item.Time == screening.Time && item.RoomId == screening.RoomId)
                                {
                                    string errorMessage = $"Lịch chiếu vào lúc {item.Time} tại phòng chiếu {item.RoomId} đã tồn tại";
                                    ModelState.AddModelError(string.Empty, errorMessage);

                                    ViewBag.Movies = new SelectList(list, "Id", "Name");
                                    return View(screening);
                                }
                            }
                        }

                        // Kiểm tra các lịch chiếu trong cùng phòng chiếu
                        if (roomScreenings.Any())
                        {
                            var latestScreening = roomScreenings.First();
                            TimeSpan latestEndTime = TimeSpan.Parse(latestScreening.EndTime);
                            TimeSpan newScreeningStartTime = TimeSpan.Parse(screening.Time);
                            TimeSpan difference = newScreeningStartTime - latestEndTime;

                            if (latestEndTime >= newScreeningStartTime || difference.TotalMinutes < 30 || difference.TotalMinutes > 60)
                            {
                                string errorMessage = $"Các lịch chiếu cùng phòng phải cách nhau ít nhất 30 phút và không quá 60 phút : {latestScreening.EndTime}";
                                ModelState.AddModelError(string.Empty, errorMessage);

                                ViewBag.Movies = new SelectList(list, "Id", "Name");
                                return View(screening);
                            }
                        }
                    }
                    // Cập nhật thông tin của đối tượng Screening
                    existingScreening.Time = screening.Time;
                    existingScreening.CinemaId = screening.CinemaId;
                    existingScreening.MovieId = screening.MovieId;
                    existingScreening.Date = screening.Date;
                    existingScreening.RoomId = screening.RoomId;
                    existingScreening.EndTime = screening.EndTime;
                    await _screeningRepository.UpdateScreeningAsync(existingScreening);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while saving data: " + ex.Message);
                }
            }
            // Lấy danh sách rạp chiếu và danh sách phim từ cơ sở dữ liệu
            var cinemas = await _cinemaRepository.GetAllAsync();
            var movies = await _movieRepository.GetAllAsync();

            // Trả về view với đối tượng Screening và danh sách rạp chiếu, danh sách phim
            ViewBag.Cinemas = new SelectList(cinemas, "Id", "Name");
            ViewBag.Movies = new SelectList(movies, "Id", "Name");
            // Nếu ModelState không hợp lệ, trả về view với dữ liệu hiện tại
            return View(screening);
        }
        [HttpGet]
        public async Task<IActionResult> Index(string Keyword)
        {
            ViewData["searching"] = Keyword;

            // Sử dụng Include để tải trước thông tin phim và rạp chiếu phim
            var screenings = _db.Screenings.Include(x => x.Movie)
                                           .Include(x => x.Cinema) // Thêm dòng này để tải trước Cinema
                                           .AsQueryable();

            // Nếu có từ khóa tìm kiếm, lọc theo tên phim
            if (!string.IsNullOrEmpty(Keyword))
            {
                screenings = screenings.Where(c => c.Movie.Name.Contains(Keyword));
            }

            // Thực hiện truy vấn bất đồng bộ và không theo dõi
            var screeningsList = await screenings.AsNoTracking().ToListAsync();

            return View(screeningsList);
        }



        public async Task<IActionResult> Delete(int id)
        {
            var screening1 = await _screeningRepository.GetByIdAsync(id);
            if (screening1 == null)
            {
                return NotFound();
            }

            var order = _db.SeatOrderDetails.Where(s => s.ScreeningId == id).ToList();

            //foreach (var item in order)
            //{
            //    item.SeatOrder = _db.SeatOrders.FirstOrDefault(s=>s.Id==item.SeatOrderId);

            if (order != null)
            {
                    return Json(new { success = false, message = "This showtime has been booked and cannot be deleted" });
                    //TempData["ErrorMessage"] = "This showtime has been booked and cannot be deleted";
                    //return RedirectToAction("Index");
                
            }
            var seat1 = _db.Seats.Where(s => s.ScreeningId == id).ToList();
            foreach (var item in seat1)
            {
                _db.Seats.Remove(item);
            }
            _db.SaveChanges();
            await _screeningRepository.DeleteAsync(id);
            return Json(new { success = true, message = "Screening deleted successfully" });
            //return RedirectToAction(nameof(Index));
        }
       





        [HttpGet]
        public async Task<IActionResult> GetFilmDates(Guid movieId)
        {
            try
            {
                var movie = await _movieRepository.GetByIdAsync(movieId);
                if (movie != null)
                {
                    return Json(new { startDate = movie.StartDate.ToString("yyyy-MM-dd"), endDate = movie.EndDate.ToString("yyyy-MM-dd") });
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving film dates: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMovieDuration(Guid movieId)
        {
            try
            {
                // Tìm bộ phim trong cơ sở dữ liệu dựa trên movieId
                var movie = await _movieRepository.GetByIdAsync(movieId);

                // Nếu bộ phim tồn tại, trả về thời lượng của bộ phim
                if (movie != null)
                {
                    return Ok(movie.DurationMinutes);
                }

                // Nếu không tìm thấy bộ phim, trả về NotFound
                return NotFound();
            }
            catch (Exception ex)
            {
                // Nếu có lỗi xảy ra, trả về lỗi Internal Server Error
                return StatusCode(500, $"An error occurred while retrieving movie duration: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSelectedCinemas(Guid movieId)
        {
            var selectedCinemas = await _cinemainCinemaRepository.GetCinemasByMovieId(movieId);
            return Json(selectedCinemas);
        }
        [HttpPost]
        public async Task<IActionResult> GetSelectedRooms(int cinemaId)
        {
            var selectedRooms = await _db.Rooms
                                         .Where(s => s.CinemaId == cinemaId)
                                         .Select(r => new { r.Id }) // Ensure you return Id and Name or whatever properties you need
                                         .ToListAsync();
            return Json(selectedRooms);
        }

    }
}

