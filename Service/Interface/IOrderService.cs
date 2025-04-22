using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Models.Enums;

namespace CycleAPI.Service.Interface
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task<OrderDto?> GetOrderByIdAsync(Guid orderId);
        Task<OrderDto?> GetOrderByOrderNumberAsync(string orderNumber);
        Task<PagedResult<OrderDto>> GetFilteredOrdersAsync(OrderQueryParameters parameters);
        Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(Guid customerId);
        Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, Guid? processedByUserId = null);
    }
}