using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProducerController : Controller
    {
        private readonly IProducerRepository _producerRepository;
        MovieContext _db;
        public ProducerController(IProducerRepository producerRepository, MovieContext db)
        {
            _producerRepository = producerRepository;
            _db = db;

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
        public async Task<IActionResult> Index()
        {
            var producers = await _producerRepository.GetAllAsync();
            return View(producers);
        }
        public async Task<IActionResult> Details(int id)
        {

            var producer = await _producerRepository.GetByIdAsync(id);
            return View("Details", producer);
        }
        public IActionResult Create()
        {
            return View("Create", new Producer());
        }
        [HttpPost]
        public async Task<IActionResult> Create(Producer newProducer, IFormFile Image)
        {
            if (ModelState.IsValid)
            {
                if (Image != null)
                {
                    // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                    newProducer.Image = await SaveImage(Image);
                }

                await _producerRepository.InsertAsync(newProducer);
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Create");
        }
        public async Task<IActionResult> Update(int id)
        {
            var actor = await _producerRepository.GetByIdAsync(id);
            if (actor == null)
            {
                return NotFound();
            }

            // Check if Image is null
            if (actor.Image != null)
            {
                actor.Image = "default.jpg"; // Set a default image path or handle accordingly
            }

            return View(actor);
        }

        // Xử lý cập nhật sản phẩm
        [HttpPost]
        public async Task<IActionResult> Update(int id, Producer producer, IFormFile Image)
        {
            ModelState.Remove("Image"); // Loại bỏ xác thực ModelState cho ImageUrl
            if (id != producer.Id)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                var existingProducer = await _producerRepository.GetByIdAsync(id);
                if (existingProducer == null)
                {
                    return NotFound();
                }

                // Cập nhật các thông tin khác của actor
                existingProducer.Name = producer.Name;
                existingProducer.Bio = producer.Bio;
                // existingActor.Description = actor.Description;

                // Kiểm tra xem có file hình ảnh được tải lên không
                if (Image == null)
                {
                    existingProducer.Image = producer.Image;
                }
                else
                {
                    // Lưu hình ảnh mới
                    existingProducer.Image = await SaveImage(Image);
                }

                // Cập nhật category vào cơ sở dữ liệu
                await _producerRepository.UpdateAsync(existingProducer);
                return RedirectToAction(nameof(Index));
            }

            return View(producer);
        }
        // Xử lý xóa actor
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _producerRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Index(string Keyword)
        {
            ViewData["searching"] = Keyword;
            var producers = _db.Producers.Select(x => x);
            if (!string.IsNullOrEmpty(Keyword))
            {
                producers = producers.Where(c => c.Name.Contains(Keyword));

            }
            return View(await producers.AsNoTracking().ToListAsync());
        }
    }
}
