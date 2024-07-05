using Microsoft.AspNetCore.Mvc;
using DoAnCoSoTL.Repositories;

namespace DoAnCoSoTL.Models
{
    public class Navbar : ViewComponent
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ICategoryRepository _categoryRepository;
		MovieContext _db;
		public Navbar(IMovieRepository movieRepository, ICategoryRepository categoryRepository, MovieContext db)
        {
            _movieRepository = movieRepository;
            _categoryRepository = categoryRepository;
			_db = db;
		}
        public IViewComponentResult Invoke()
        {
            var listTask = _categoryRepository.GetAllAsync();
            listTask.Wait(); // Chờ cho tác vụ hoàn thành

            var list = listTask.Result;
            return View(list);
        }
    }
}
