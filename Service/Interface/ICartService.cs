using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Service.Interface
{
    public interface ICartService
    {
        Task<CartDto?> GetCartByIdAsync(Guid cartId);
        Task<CartDto?> GetActiveCartAsync(Guid customerId);
        Task<CartDto> CreateCartAsync(Guid customerId, string? sessionId = null);
        Task<IEnumerable<CartDto>> GetAllActiveCartsAsync();
        Task<IEnumerable<CartDto>> SearchCartsByCustomerNameAsync(string name);
        Task<CartItemDto> AddItemToCartAsync(Guid cartId, AddCartItemDto addCartItemDto);
        Task<CartItemDto> UpdateCartItemQuantityAsync(Guid cartItemId, UpdateCartItemDto updateCartItemDto);
        Task<bool> RemoveItemFromCartAsync(Guid cartItemId);
        Task<bool> ClearCartAsync(Guid cartId);
        Task<decimal> CalculateCartTotalAsync(Guid cartId);
        Task<PagedResult<CartDto>> GetFilteredCartsAsync(CartQueryParameters parameters);
        Task<bool> ExistsAsync(Guid cartId);
    }
}
