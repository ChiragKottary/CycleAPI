using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;

namespace CycleAPI.Repositories.Interface
{
    public interface IOrderItemRepository
    {
        Task<OrderItem?> GetByIdAsync(Guid id);
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<OrderItem>> GetByCycleIdAsync(Guid cycleId);
        Task<OrderItem> AddAsync(OrderItem orderItem);
        Task<OrderItem> UpdateAsync(OrderItem orderItem);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<(IEnumerable<OrderItem> Items, int TotalCount)> GetFilteredAsync(OrderItemQueryParameters parameters);
        Task<bool> SaveChangesAsync();
    }
}