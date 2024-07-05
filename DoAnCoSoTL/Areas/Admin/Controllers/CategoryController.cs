using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoAnCoSoTL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        MovieContext _db;
        public CategoryController(ICategoryRepository categoryRepository, MovieContext db)
        {
            _categoryRepository = categoryRepository;
            _db = db;

        }
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
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
            return View("Create", new Category());
        }
        //action xử lý thêm category
        [HttpPost]

        public async Task<IActionResult> Create(Category newCategory, IFormFile Image)
        {
            if (!ModelState.IsValid)
            {
                if (Image != null)
                {
                    // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                    newCategory.Image = await SaveImage(Image);
                }

                await _categoryRepository.InsertAsync(newCategory);
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Create");
        }
        public async Task <IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            if (category.Image != null)
            {
                category.Image = "default.jpg"; // Set a default image path or handle accordingly
            }
            var movies = _db.Movies.Where(s=>s.Cat_Id ==  category.Id);
            if(movies.Any())
            {
                foreach (var movie in movies)
                {
                    var order = _db.SeatOrderDetails.Where(s=>s.MovieId == movie.Id);
                    if (order.Any())
                    {
                        TempData["ErrorMessage"] = "This category has been booked and cannot be edited";
                        return RedirectToAction("Index");
                    }
                }
            }
            return View(category);
        }

        // Xử lý cập nhật sản phẩm
        [HttpPost]
        public async Task<IActionResult> Update(int id, Category category, IFormFile Image)
        {
            ModelState.Remove("Image"); // Loại bỏ xác thực ModelState cho ImageUrl
            if (id != category.Id)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound();
                }

                // Cập nhật các thông tin khác của category
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;

                // Kiểm tra xem có file hình ảnh được tải lên không
                if (Image == null)
                {
                    existingCategory.Image = category.Image;
                }
                else
                {
                    // Lưu hình ảnh mới
                    existingCategory.Image = await SaveImage(Image);
                }

                // Cập nhật category vào cơ sở dữ liệu
                await _categoryRepository.UpdateAsync(existingCategory);
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }


        // Xử lý xóa sản phẩm
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var movies = _db.Movies.Where(s => s.Cat_Id == category.Id);
            if (movies.Any())
            {
                foreach (var movie in movies)
                {
                    var order = _db.SeatOrderDetails.Where(s => s.MovieId == movie.Id);
                    if (order.Any())
                    {
                        return Json(new { success = false, message = "This category has been set and cannot be deleted" });
                        //return RedirectToAction("Index", "Movie");
                    }
                    return Json(new { success = true, message = "Category deleted successfully" });
                }
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
        {

            Category categories = await _categoryRepository.GetByIdAsync(id);
            return View("Details", categories);
        }


    }
}
