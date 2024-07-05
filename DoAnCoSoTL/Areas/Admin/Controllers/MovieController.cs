using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using DoAnCoSoTL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Security.Principal;

namespace DoAnCoSoTL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class MovieController : Controller
    {
        private readonly IMovieActorRepository _movieActorRepository;
        private readonly IMovieInCinemaRepository _moviesInCinemaRepository;
        private readonly ICinemaRepository _cinemaRepository;
        private readonly IProducerRepository _producerRepository;
        private readonly IActorRepository _actorRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMovieRepository _movieRepository;
        MovieContext _db;
        public MovieController(ICategoryRepository categoryRepository, IMovieRepository movieRepository, MovieContext db,
            IMovieActorRepository movieactorRepository,
            ICinemaRepository cinemaRepository, IProducerRepository producerRepository, IActorRepository actorRepository, IMovieInCinemaRepository movieInCinemaRepository)
        {
            _categoryRepository = categoryRepository;
            _db = db;
            _movieRepository = movieRepository;
            _movieActorRepository = movieactorRepository;
            _producerRepository = producerRepository;
            _actorRepository = actorRepository;
            _cinemaRepository = cinemaRepository;
            _moviesInCinemaRepository = movieInCinemaRepository;
        }
        static Guid iid;
        public async Task<IActionResult> Index()
        {
            var movies = await _movieRepository.GetAllAsync();
            return View(movies);
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
        public IActionResult Create()
        {
            ViewBag.Cinemas = new SelectList(_db.Cinemas.ToList(), "Id", "Name");
            ViewBag.Categories = new SelectList(_db.Categories.ToList(), "Id", "Name");
            ViewBag.Actors = new SelectList(_db.Actors.ToList(), "Id", "Name");
            ViewBag.Producers = new SelectList(_db.Producers.ToList(), "Id", "Name");
            List<SelectListItem> Age;
            Age = new List<SelectListItem>
            {
               new SelectListItem { Value = "P", Text = "All Ages" },
               new SelectListItem { Value = "13", Text = "13 or above"},
               new SelectListItem { Value = "16", Text = "16 or above"},
               new SelectListItem { Value = "18", Text = "18 or above"},
               new SelectListItem { Value = "K", Text = "Under 13 with Guardian" }
            };
            ViewBag.Ages = Age;
            return View(new MovieViewModel());
        }
        //action xử lý thêm movie


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieViewModel movievm, IFormFile Image)
        {
            // Kiểm tra tính hợp lệ của ngày khởi chiếu và kết thúc
            TimeSpan date = movievm.EndDate.Date - movievm.StartDate.Date;
            TimeSpan startDate = movievm.StartDate.Date - DateTime.Now.Date;
            if (movievm.StartDate <= DateTime.Now.Date)
            {
                ModelState.AddModelError("StartDate", "The movie start date must be greater than the current date.");
            }
            var list = _db.Movies.ToList();
            foreach(var item in list)
            {
                if(item.Name == movievm.Name && item.Cat_Id == movievm.Category_Id)
                {
                    ModelState.AddModelError("Name", "Movies cannot have the same name if they belong to the same genre.");
                }    
            }
            if (date.Days < 10)
            {
                ModelState.AddModelError("EndDate", "The end date must be at least 10 days greater than the start date.");
            }
            if (startDate.Days < 7 || startDate.Days > 30)
            {
                ModelState.AddModelError("StartDate", "Movies must be added at least 7 days in advance and no more than 1 month in advance.");
            }

            // Kiểm tra tính hợp lệ của giá vé
            if (movievm.Price <= 30000)
            {
                ModelState.AddModelError("Price", "Ticket price must be greater than 30000.");
            }

            // Kiểm tra tính hợp lệ của thời lượng phim
            if (movievm.DurationMinutes < 60)
            {
                ModelState.AddModelError("DurationMinutes", "The duration of the film must be greater than 60 minutes.");
            }

           

            // Nếu có bất kỳ lỗi nào trong ModelState, trả về view Create với dữ liệu hiện tại và thông báo lỗi
            if (!ModelState.IsValid)
            {
                // Load lại các dropdownlist cần thiết
                ViewBag.Cinemas = new SelectList(_db.Cinemas.ToList(), "Id", "Name");
                ViewBag.Categories = new SelectList(_db.Categories.ToList(), "Id", "Name");
                ViewBag.Actors = new SelectList(_db.Actors.ToList(), "Id", "Name");
                ViewBag.Producers = new SelectList(_db.Producers.ToList(), "Id", "Name");
                ViewBag.Ages = new List<SelectListItem>
                {
                    new SelectListItem { Value = "P", Text = "All Ages" },
                    new SelectListItem { Value = "13", Text = "13 or above"},
                    new SelectListItem { Value = "16", Text = "16 or above"},
                    new SelectListItem { Value = "18", Text = "18 or above"},
                    new SelectListItem { Value = "K", Text = "Under 13 with Guardian" }
                };

                // Trả về view Create với dữ liệu hiện tại và thông báo lỗi
                return View("Create", movievm);
            }

            // Nếu ModelState hợp lệ, thêm bộ phim vào cơ sở dữ liệu và chuyển hướng đến trang Index
            var movie = new Movie
            {
                Id = Guid.NewGuid(),
                Name = movievm.Name,
                Description = movievm.Description,
                Price = movievm.Price,
                StartDate = movievm.StartDate,
                EndDate = movievm.EndDate,
                Cat_Id = movievm.Category_Id,
                Producer_Id = movievm.Producer_Id,
                Trailer = movievm.Trailer,
                Age = movievm.Age,
                DurationMinutes = movievm.DurationMinutes
            };

            if (Image != null)
            {
                movie.Image = await SaveImage(Image);
            }

            await _movieRepository.InsertAsync(movie);

            // Thêm diễn viên vào bộ phim
            if (movievm.ActorIds != null && movievm.ActorIds.Any())
            {
                foreach (var actorId in movievm.ActorIds)
                {
                    await _movieActorRepository.InsertMovieActorAsync(movie.Id, actorId);
                }
            }

            // Thêm rạp vào bộ phim
            if (movievm.CinemaIds != null && movievm.CinemaIds.Any())
            {
                foreach (var cinemaId in movievm.CinemaIds)
                {
                    await _moviesInCinemaRepository.InsertMoviesInCinemaAsync(movie.Id, cinemaId);
                }
            }

            // Chuyển hướng đến trang Index
            return RedirectToAction("Index", "Movie");
        }




        public ActionResult Update(Guid id)

        {
            
            iid = id;
            var seat = _db.SeatOrderDetails.FirstOrDefault(x => x.MovieId == id);
            if (seat != null)
            {
                ModelState.AddModelError(string.Empty, "This movie has been set and cannot be edited");
                TempData["ErrorMessage"] = "This movie has been set and cannot be edited";
                return RedirectToAction("Index", "Movie");
            }
            MovieViewModel Moviemodel = _movieRepository.GetMovieByIdAdmin(id);
            List<SelectListItem> Age;
            Age = new List<SelectListItem>
            {
               new SelectListItem { Value = "P", Text = "All Ages" },
               new SelectListItem { Value = "13", Text = "13 or above"},
               new SelectListItem { Value = "16", Text = "16 or above"},
               new SelectListItem { Value = "18", Text = "18 or above"},
               new SelectListItem { Value = "K", Text = "Under 13 with Guardian" }
            };
            ViewBag.Ages = Age;
            ViewBag.Cinemas = new SelectList(_db.Cinemas.ToList(), "Id", "Name");
            ViewBag.Categories = new SelectList(_db.Categories.ToList(), "Id", "Name");
            ViewBag.Actors = new SelectList(_db.Actors.ToList(), "Id", "Name");
            ViewBag.Producers = new SelectList(_db.Producers.ToList(), "Id", "Name");



            return View("Update", Moviemodel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(MovieViewModel editMovie, IFormFile Image)
        {
            // Kiểm tra tính hợp lệ của dữ liệu nhập vào
            if (editMovie.Price <= 30000) //Giá vé phải lớn hơn 30.000vnd
            {
                ModelState.AddModelError("Price", "Ticket price must be greater than 30000.");
            }
            var list = _db.Movies.ToList();
            foreach (var item in list)
            {
                if (item.Id != iid && item.Name == editMovie.Name && item.Cat_Id == editMovie.Category_Id) // Không thể tồn tại các bộ phim trùng tên thuộc cùng 1 thể loại
                {
                    ModelState.AddModelError("Name", "Movies cannot have the same name if they belong to the same genre.");
                }
            }
            if (editMovie.DurationMinutes < 60) //Thời lượng chiếu của 1 bộ phim phải lớn hơn hoặc bằng 60 phút
            {
                ModelState.AddModelError("DurationMinutes", "The duration of the film must be greater than 60 minutes.");
            }
            if (editMovie.ActorIds == null || !editMovie.ActorIds.Any()) //Phải chọn ít nhất 1 diễn viên khi thêm phim
            {
                ModelState.AddModelError("ActorIds", "Please choose at least one actor.");
            }
            if (editMovie.CinemaIds == null || !editMovie.CinemaIds.Any()) // Phải chọn ít nhất 1 rạp khi thêm phim
            {
                ModelState.AddModelError("CinemaIds", "Please choose at least one cinema.");
            }
            TimeSpan date = editMovie.EndDate.Date - editMovie.StartDate.Date;
            TimeSpan startDate = editMovie.StartDate.Date - DateTime.Now.Date;
            if (editMovie.StartDate <= DateTime.Now.Date)
            {
                ModelState.AddModelError("StartDate", "The movie start date must be greater than the current date."); // Ngày khởi chiếu phim phải lớn hơn ngày hiện tại
            }
            if (date.Days < 10)
            {
                ModelState.AddModelError("EndDate", "The end date must be at least 10 days greater than the start date.");//Ngày kết thúc chiếu phải lớn hơn ít nhất 10 ngày so với ngày bắt đầu khởi chiếu
            }
            if (startDate.Days < 7 || startDate.Days > 30) // Khi thêm mới 1 bộ phim phải thêm trước ít nhất 7 ngày và bé hơn 30 ngày so với ngày hiện tại
            {
                ModelState.AddModelError("StartDate", "Movies must be added at least 7 days in advance and no more than 1 month in advance.");
            }

            // Nếu ModelState không hợp lệ, load lại các dropdown list và trả về view Update với dữ liệu hiện tại và thông báo lỗi
            if (!ModelState.IsValid)
            {
                ViewBag.Cinemas = new SelectList(_db.Cinemas.ToList(), "Id", "Name");
                ViewBag.Categories = new SelectList(_db.Categories.ToList(), "Id", "Name");
                ViewBag.Actors = new SelectList(_db.Actors.ToList(), "Id", "Name");
                ViewBag.Producers = new SelectList(_db.Producers.ToList(), "Id", "Name");
                ViewBag.Ages = new List<SelectListItem>
                {
                    new SelectListItem { Value = "P", Text = "All Ages" },
                    new SelectListItem { Value = "13", Text = "13 or above" },
                    new SelectListItem { Value = "16", Text = "16 or above" },
                    new SelectListItem { Value = "18", Text = "18 or above" },
                    new SelectListItem { Value = "K", Text = "Under 13 with Guardian" }
                };

                // Trả về view Update với dữ liệu hiện tại và thông báo lỗi
                return View("Update", editMovie);
            }

            // Nếu ModelState hợp lệ, cập nhật phim vào cơ sở dữ liệu
            await _movieRepository.UpdateAsync(editMovie, iid, Image);
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Delete(Guid id)
        {
            var seat = _db.SeatOrderDetails.FirstOrDefault(x => x.MovieId == id);
            if (seat != null)
            {
                return Json(new { success = false, message = "This movie has been set and cannot be deleted" });
                //return RedirectToAction("Index", "Movie");
            }
            await _movieRepository.DeleteAsync(id);
            return Json(new { success = true, message = "Movie deleted successfully" });
            //return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(Guid id)
        {
            //Movie movies = await _movieRepository.GetByIdAsync(id);
            //return View("Details", movies);
            var movie = await _movieRepository.GetByIdAsync(id);

            TimeSpan dateDifference = movie.StartDate.Date - DateTime.Now.Date;
            ViewBag.Date = dateDifference.Days;
            var producer = _db.Producers.SingleOrDefault(s => s.Id == movie.Producer_Id);
            var actor = _db.MovieActors.Where(s => s.MovieId == movie.Id).ToList();
            foreach (var item in actor)
            {
                item.Actor = _db.Actors.SingleOrDefault(s => s.Id == item.ActorId);
            }
            ViewBag.Producer = producer;
            ViewBag.Actor = actor;

            if (movie == null)
            {
                return NotFound();
            }
            if (movie.Cat_Id != null)
            {
                movie.Category = await _categoryRepository.GetByIdAsync(movie.Cat_Id);
            }
            return View(movie);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string Keyword)
        {
            ViewData["searching"] = Keyword;
            var movies = _db.Movies.Select(x => x);
            if (!string.IsNullOrEmpty(Keyword))
            {
                movies = movies.Where(c => c.Name.Contains(Keyword));
            }
            var moviesList = await movies.AsNoTracking().ToListAsync();
            return View(moviesList);
        }

    }
}
