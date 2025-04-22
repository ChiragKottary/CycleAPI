using CycleAPI.Models.Domain;

namespace CycleAPI.Repositories.Interface
{
    public interface ITokenRepository
    {
        Task<string> CreateTokenAsync(User user, List<string> roles);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
    }
}
