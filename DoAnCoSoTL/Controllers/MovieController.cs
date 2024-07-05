using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using DoAnCoSoTL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace DoAnCoSoTL.Controllers
{
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


        #region User
        #region Index
        public async Task<IActionResult> Index()
        {
            
            var movies = await _movieRepository.GetAllAsync();
            foreach (var movie in movies)
            {
                if (movie.Cat_Id != null)
                {
                    movie.Category = await _categoryRepository.GetByIdAsync(movie.Cat_Id);
                }
                movie.TotalQuantitySold = await _movieRepository.TotalQuantitySoldAsync(movie.Id);
            }
            //TimeSpan dateDifference = movie.StartDate.Date - DateTime.Now.Date;
            //ViewBag.Date = dateDifference.Days;
            return View(movies);
        }
        [HttpPost]
        public async Task<IActionResult> Index(int catid, string keywords)
        {
            var demoContext = _db.Movies.Include(p => p.Category).Where(p => p.Name.Contains(keywords) && p.Cat_Id == catid);
            return View(await demoContext.ToListAsync());
        }
        #endregion
        #region Details
        public async Task<IActionResult> Display(Guid id)
        {
            
            
            var movie = await _movieRepository.GetByIdAsync(id);

            TimeSpan dateDifference = movie.StartDate.Date - DateTime.Now.Date;
            ViewBag.Date = dateDifference.Days;
            var producer = _db.Producers.SingleOrDefault(s => s.Id == movie.Producer_Id);
            var actor = _db.MovieActors.Where(s=>s.MovieId == movie.Id).ToList();
            foreach(var item in  actor)
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
        #endregion
        #endregion
        public async Task<IActionResult> SearchResult(string keyword, int catid = 0)
        {
            var searchResults = await _movieRepository.SearchAsync(keyword);
            foreach(var item in  searchResults)
            {
                item.Category = await _categoryRepository.GetByIdAsync(item.Cat_Id);
            }

            if (catid != 0)
            {
                searchResults = searchResults.Where(p => p.Cat_Id == catid);
            }

            return View("SearchResult", searchResults);
        }


        public IEnumerable<Movie> GetProductsByCategoryId(int categoryId)
        {
            // Truy vấn cơ sở dữ liệu để lấy danh sách sản phẩm có CategoryId tương ứng
            // Ví dụ: Sử dụng Entity Framework Core để thực hiện truy vấn dữ liệu từ cơ sở dữ liệu
            return _db.Movies.Where(p => p.Cat_Id == categoryId).ToList();
        }
        public async Task<IActionResult> FilterByCategory(int categoryId)
        {
            // Truy vấn cơ sở dữ liệu để lấy danh sách sản phẩm có CategoryId tương ứng
            var movies = _db.Movies.Where(p => p.Cat_Id == categoryId).ToList();
            foreach (var movie in movies)
            {
                if (movie.Cat_Id != null)
                {
                    movie.Category = await _categoryRepository.GetByIdAsync(movie.Cat_Id);
                }
            }
            // Trả về một PartialView chứa danh sách sản phẩm đã lọc
            return View("FilterByCategory", movies);
        }

    }
}