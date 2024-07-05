using DoAnCoSoTL.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Repositories
{
    public class CartRepository : ICartRepository
    {
        MovieContext db;
        public CartRepository(MovieContext db)
        {
            this.db = db;
        }
        public async Task DeleteAsync(int id)
        {
            var cartToDelete = await db.Cart.FirstOrDefaultAsync(c => c.Id == id);

            if (cartToDelete != null)
            {
                db.Cart.Remove(cartToDelete);
                await db.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<Cart>> GetAllAsync()
        {
            return await db.Cart.ToListAsync();
        }

        public async Task UpdateAsync(Cart editCart)
        {
            db.Cart.Update(editCart);
            await db.SaveChangesAsync();
        }
        public async Task<IEnumerable<Cart>> GetDataAsync(Cart cart)
        {
            var carts = await db.Cart
                .Where(w => w.UserId == cart.UserId && w.MovieId == cart.MovieId)
                .ToListAsync();
            return carts;
        }
        public async Task InsertAsync(Cart cart)
        {
            await db.Cart.AddAsync(cart);
            await db.SaveChangesAsync();
        }

    }
}
