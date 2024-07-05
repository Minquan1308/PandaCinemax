using Microsoft.AspNetCore.Mvc;
using DoAnCoSoTL.Models;

namespace DoAnCoSoTL.Repositories
{
    public interface IOrderRepository
    {
        //Task<IEnumerable<Order>> GetAllAsync();
        //Task<Order> GetByIdAsync(int id);
        ////Task AddAsync(Product product);
        ////Task UpdateAsync(Product product);
        ////Task DeleteAsync(int id);
        Task<IEnumerable<SeatOrder>> GetAllAsync();
        Task<SeatOrder> GetByIdAsync(Guid id);
        Task UpdateAsync(SeatOrder order);
        Task DeleteAsync(Guid id);
        IEnumerable<SeatOrder> GetAll();
        Task<int> GetTotalQuantitySoldAsync(Guid movieId);

        //IEnumerable<SeatOrder> GetListOrderByUserId(int id);

    }
}
