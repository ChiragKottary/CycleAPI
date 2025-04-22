using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.Enums;

namespace CycleAPI.Repositories.Interface
{
    public interface IOrderRepository
    {
        Task<Order?> GetOrderByIdAsync(Guid id);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(Order order);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> UpdateStatusAsync(Guid id, OrderStatus status);
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetFilteredAsync(OrderQueryParameters parameters);
        Task<bool> SaveChangesAsync();
    }
}