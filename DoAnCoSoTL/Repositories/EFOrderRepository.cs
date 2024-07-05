using DoAnCoSoTL.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Repositories
{
    public class EFOrderRepository : IOrderRepository
    {
        
        private readonly MovieContext _context;
        public EFOrderRepository(MovieContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<SeatOrder>> GetAllAsync()
        {
            var orders = await _context.SeatOrders
                .Include(o => o.ApplicationUser)
                .Include(o => o.SeatOrderDetails)
                    //.ThenInclude(od => od.Seat.Movie)
                .ToListAsync();

            // Tính toán TotalQuantitySold cho mỗi đơn hàng
            foreach (var order in orders)
            {
                order.TotalQuantitySold = await GetTotalQuantitySoldAsync(order.Id);
            }

            return orders;
        }
        public async Task<SeatOrder> GetByIdAsync(Guid id) //trả về một đối tượng Order khi tác vụ hoàn thành
        {
            return await _context.SeatOrders
        .Include(o => o.ApplicationUser)
        .Include(o => o.SeatOrderDetails)
            //.ThenInclude(od => od.Seat.Screening.Movie)
        .FirstOrDefaultAsync(o => o.Id == id);
        }
        public async Task UpdateAsync(SeatOrder order)
        {
            _context.SeatOrders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var order = await _context.SeatOrders.FindAsync(id);
            _context.SeatOrders.Remove(order);
            await _context.SaveChangesAsync();
        }
        //public async IEnumerable<SeatOrder> GetListOrderByUserId(string id)
        //{
        //    var order = await _context.SeatOrders.Where(s => s.UserId == id);
        //    return order;
        //}


        public IEnumerable<SeatOrder> GetAll()
        {
            // Lấy tất cả các đơn hàng từ cơ sở dữ liệu và trả về
            return _context.SeatOrders.ToList();
        }
        // Cài đặt phương thức tính tổng số lượng đã bán của một sản phẩm
        public async Task<int> GetTotalQuantitySoldAsync(Guid movieId)
        {
            // Lấy tổng số lượng đã bán của sản phẩm dựa trên productId từ bảng OrderDetails
            var totalQuantitySold = await _context.SeatOrderDetails
                .Where(od => od.MovieId == movieId)
                .SumAsync(od => od.Quantity);

            return totalQuantitySold;
        }
    }
}
