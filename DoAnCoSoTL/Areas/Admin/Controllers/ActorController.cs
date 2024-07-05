using DoAnCoSoTL.Migrations;
using DoAnCoSoTL.Models;
using DoAnCoSoTL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ActorController : Controller
    {
        private readonly IActorRepository _actorRepository;
        MovieContext _db;
        public ActorController(IActorRepository actorRepository, MovieContext db)
        {
            _actorRepository = actorRepository;
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
            var actors = await _actorRepository.GetAllAsync();
            return View(actors);
        }
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {

            Actor actors = await _actorRepository.GetByIdAsync(id);
            return View("Details", actors);
        }
        public IActionResult Create()
        {
            return View("Create", new Actor());
        }

        //action xử lý thêm category
        [HttpPost]
        public async Task<IActionResult> Create(Actor newActor, IFormFile Image)
        {
            if (!ModelState.IsValid)
            {
                //var errorCount = ModelState.Values.SelectMany(v => v.Errors).Count();
                //foreach (var state in ModelState)
                //{
                //    Console.WriteLine($"Key: {state.Key}");
                //    foreach (var error in state.Value.Errors)
                //    {
                //        Console.WriteLine($"Error: {error.ErrorMessage}");
                //        if (error.Exception != null)
                //        {
                //            Console.WriteLine($"Exception: {error.Exception.Message}");
                //        }
                //    }
                //}

                // Ensure ViewBag or other necessary data is populated
                // PopulateViewData();
                return View(newActor);
            }

            else
            {
                if (Image != null)
                {
                    // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                    newActor.Image = await SaveImage(Image);
                }
                await _actorRepository.InsertAsync(newActor);
                return RedirectToAction(nameof(Index));

            }

        }
        public async Task<IActionResult> Update(int id)
        {
            var actor = await _actorRepository.GetByIdAsync(id);
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
        public async Task<IActionResult> Update(int id, Actor actor, IFormFile Image)
        {
            ModelState.Remove("Image"); // Loại bỏ xác thực ModelState cho ImageUrl
            if (id != actor.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var existingActor = await _actorRepository.GetByIdAsync(id);
                if (existingActor == null)
                {
                    return NotFound();
                }

                // Cập nhật các thông tin khác của actor
                existingActor.Name = actor.Name;
                existingActor.Bio = actor.Bio;
               // existingActor.Description = actor.Description;

                // Kiểm tra xem có file hình ảnh được tải lên không
                if (Image == null)
                {
                    existingActor.Image = actor.Image;
                }
                else
                {
                    // Lưu hình ảnh mới
                    existingActor.Image = await SaveImage(Image);
                }

                // Cập nhật category vào cơ sở dữ liệu
                await _actorRepository.UpdateAsync(existingActor);
                return RedirectToAction(nameof(Index));
            }

            return View(actor);
        }
        // Xử lý xóa actor
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _actorRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        //public async Task<IActionResult> Index(string Keyword)
        //{
        //    ViewData["searching"] = Keyword;
        //    var actors = _db.Actors.Select(x => x);
        //    if (!string.IsNullOrEmpty(Keyword))
        //    {
        //        actors = actors.Where(c => c.Name.Contains(Keyword));

        //    }
        //    return View(await actors.AsNoTracking().ToListAsync());
        //}
        [HttpGet]
        public async Task<IActionResult> Index(string Keyword)
        {
            ViewData["searching"] = Keyword;
            var actors = _db.Actors.Select(x => x);
            if (!string.IsNullOrEmpty(Keyword))
            {
                actors = actors.Where(c => c.Name.Contains(Keyword));
            }
            var actorsList = await actors.AsNoTracking().ToListAsync();
            return View(actorsList);
        }


    }
}
