using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;

namespace CycleAPI.Repositories.Interface
{
    public interface ICartRepository
    {
        Task<Cart?> GetByIdAsync(Guid cartId);
        Task<Cart?> GetActiveByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<Cart>> GetAllActiveAsync();
        Task<IEnumerable<Cart>> SearchByCustomerNameAsync(string name);
        Task<Cart> CreateCartAsync(Guid customerId, string sessionId);
        Task<Cart> AddAsync(Cart cart);
        Task<Cart> UpdateAsync(Cart cart);
        Task<bool> DeleteAsync(Guid cartId);
        Task<bool> ExistsAsync(Guid cartId);
        Task<(IEnumerable<Cart> Carts, int TotalCount)> GetFilteredAsync(CartQueryParameters parameters);
        Task<bool> SaveChangesAsync();
    }
}
