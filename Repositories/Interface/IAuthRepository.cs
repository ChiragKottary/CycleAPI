using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;

namespace CycleAPI.Repositories.Interface
{
    public interface IAuthRepository
    {
        Task<RegisterResponseDto> AddAsync(User user);
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> SaveChangesAsync();
        Task<Guid> GetUserIdByEmailAsync(string email);
        
        // Employee management methods
        Task<IEnumerable<User>> GetAllEmployeesAsync();
        Task<bool> DeactivateEmployeeAsync(Guid id);
        Task<bool> ActivateEmployeeAsync(Guid id);
        
        // Role management
        Task<bool> RoleExistsAsync(Guid roleId);
    }
}
