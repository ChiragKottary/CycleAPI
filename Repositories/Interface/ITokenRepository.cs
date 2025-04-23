using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;

namespace CycleAPI.Repositories.Interface
{
    public interface ITokenRepository
    {
        Task<string> CreateTokenAsync(User user, List<string> roles);
        Task<string> CreateCustomerTokenAsync(CustomerDto customer);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
    }
}
