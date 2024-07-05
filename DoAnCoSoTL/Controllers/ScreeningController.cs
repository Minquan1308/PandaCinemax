using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Controllers
{
    public class ScreeningController : Controller
    {
        private readonly MovieContext _context;
        private readonly ICinemaRepository _cinemaRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IScreeningRepository _screeningRepository;


        public ScreeningController(MovieContext context, ICinemaRepository cinemaRepository, IMovieRepository movieRepository)
        {
            _context = context;
            _cinemaRepository = cinemaRepository;
            _movieRepository = movieRepository;
        }

        // GET: Screening/Index
        public async Task<IActionResult> Index(Guid movieId, DateTime? date)
        {
            var oldscreening = _context.Screenings.Where(s=>s.MovieId == movieId).ToList();
            foreach (var item in oldscreening)
            {
                if (item.Date < DateTime.Today.Date)
                {
                    var seats = _context.Seats.Where(s => s.ScreeningId == item.Id).ToList();
                    foreach (var seat in seats)
                    {
                        _context.Seats.Remove(seat);
                    }
                    _context.SaveChanges();
                }
            }
            if (date == null)
            {
                date = DateTime.Today.Date; // Nếu không có ngày nào được chọn, mặc định là ngày hiện tại
            }

            var screenings = _context.Screenings.Where(s => s.MovieId == movieId && s.Date.Date == date).OrderBy(s => s.Time).ToList();
            var filteredScreenings = new List<Screening>();
            var currentTime = DateTime.Now.TimeOfDay; // Lấy giờ phút hiện tại

            foreach (var screening in screenings)
            {
                screening.Movie = await _movieRepository.GetByIdAsync(screening.MovieId);
                screening.Cinema = await _cinemaRepository.GetByIdAsync(screening.CinemaId);
                

                // Chuyển đổi giờ kết thúc từ string sang TimeSpan hoặc DateTime
                TimeSpan startTime;
                if (TimeSpan.TryParse(screening.Time, out startTime)) // assuming EndTime is a string property
                {
                    // Chỉ so sánh giờ kết thúc với giờ phút hiện tại nếu ngày chiếu là ngày hôm nay
                    if (screening.Date == DateTime.Today.Date)
                    {
                        // So sánh giờ kết thúc với giờ phút hiện tại
                        if (startTime >= currentTime)
                        {
                            // Thêm vào danh sách các lịch chiếu có giờ kết thúc lớn hơn giờ phút hiện tại
                            filteredScreenings.Add(screening);
                        }
                        //var seats = _context.Seats.Where(s=>s.ScreeningId == screening.Id).ToList();
                        //foreach(var seat in seats)
                        //{
                        //    _context.Seats.Remove(seat);
                        //}  
                        //_context.SaveChanges();
                    }
                    else
                    {
                        // Nếu ngày chiếu không phải là ngày hôm nay, thêm tất cả các lịch chiếu
                        filteredScreenings.Add(screening);
                        
                    }
                }
            }
            var movie = await _movieRepository.GetByIdAsync(movieId);
            if (movie == null)
            {
                return NotFound();
            }

            ViewBag.MovieId = movieId;
            ViewBag.MovieName = movie.Name;
            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");

            return View(filteredScreenings);
        }
        [HttpGet]
        public async Task<IActionResult> Search()
        {
            var cinemas = await _context.Cinemas.ToListAsync();
            ViewBag.Cinemas = cinemas;
            // Mặc định rạp và ngày hiện tại
            int? defaultCinemaId = _context.Cinemas.FirstOrDefault()?.Id;
            DateTime? defaultDate = DateTime.Now.Date;

            // Gọi action Search với các giá trị mặc định
            return await Search(defaultCinemaId, defaultDate);
        }
        [HttpPost]
        public async Task<IActionResult> Search(int? id, DateTime? date)
        {
            // Nếu không có ngày được chọn, mặc định là ngày hiện tại
            //if (date == null)
            //{
            //    date = DateTime.Now.Date;
            //}

            //// Nếu không có rạp được chọn, mặc định là rạp đầu tiên
            //if (id == null)
            //{
            //    id = _context.Cinemas.FirstOrDefault()?.Id;
            //}
            var cinemas = await _context.Cinemas.ToListAsync();
            ViewBag.Cinemas = cinemas;

            // Chỉ gán lại giá trị mặc định cho ViewBag nếu người dùng không cung cấp giá trị mới
            if (ViewBag.SelectedDate == null || ViewBag.SelectedCinemaId == null)
            {
                ViewBag.SelectedDate = date;
                ViewBag.SelectedCinemaId = id;
            }
            List<Screening> list = new List<Screening>();
            TimeSpan endTime;
            var currentTime = DateTime.Now.TimeOfDay; // Lấy giờ phút hiện tại
            var screenings = _context.Screenings
                .Where(s => s.CinemaId == id && s.Date == date)
                .Include(s => s.Movie) // Assuming Screening has a Movie navigation property
                .Include(s => s.Cinema) // Assuming Screening has a Cinema navigation property
                .ToList();
             foreach (var screening in screenings)
            {
                screening.Movie.Category = _context.Categories.SingleOrDefault(s => s.Id == screening.Movie.Cat_Id);

            }
            if (date == DateTime.Now.Date)
            {
                foreach (var screening in screenings)
                {
                    screening.Movie.Category = _context.Categories.SingleOrDefault(s => s.Id == screening.Movie.Cat_Id);
                    {
                        if (TimeSpan.TryParse(screening.Time, out endTime))
                        {
                            if (endTime > currentTime)
                            {
                                list.Add(screening);
                            }
                        }

                    }
                }
                return View("Search", list);

            }
            return View("Search", screenings);
        }


        // GET: Screening/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var screening = _context.Screenings.FirstOrDefault(s => s.Id == id);
            if (screening == null)
            {
                return NotFound();
            }

            return View(screening);
        }
    }
}
