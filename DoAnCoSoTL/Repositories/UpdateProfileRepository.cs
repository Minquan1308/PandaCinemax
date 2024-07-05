using DoAnCoSoTL.Models;

namespace DoAnCoSoTL.Repositories
{
    public class UpdateProfileRepository : IUpdateProfileRepository
    {
        MovieContext db;
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName); // Thay  đổi đường dẫn theo cấu hình của bạn 
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối }

        }
        public UpdateProfileRepository(MovieContext _db)
        {
            db = _db;
        }
        public ApplicationUser GetById(string id)
        {
            var user = db.Users.FirstOrDefault(x => x.Id == id);
            return user;
        }
        //Update user profile -----------------------
        public async Task<int> updateAsync(string id, ApplicationUser UpdateUser, IFormFile Image)
        {
            if (Image != null)
            {
                // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                UpdateUser.Image = await SaveImage(Image);
            }
            var user = db.Users.SingleOrDefault(u => u.Id == id);

            user.FullName = UpdateUser.FullName;
            //if (Image.Count != 0)
            //{
            //    user.Image = UpdateUser.Image;
            //}
            user.Address = UpdateUser.Address;
            int raws = db.SaveChanges();
            return raws;

        }
        //Add new user ---------------------------------------------------
        public async Task<int> insert(ApplicationUser NewUser, IFormFile Image)
        {
            if (Image != null)
            {
                // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                NewUser.Image = await SaveImage(Image);
            }
            db.Add(NewUser);
            return db.SaveChanges();
        }

    }
}
