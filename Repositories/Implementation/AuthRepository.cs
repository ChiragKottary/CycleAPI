using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CycleAPI.Repositories.Implementation
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AuthDbContext _context;
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(
            AuthDbContext context, 
            ITokenRepository tokenRepository,
            ILogger<AuthRepository> logger)
        {
            _context = context;
            _tokenRepository = tokenRepository;
            _logger = logger;
        }

        public async Task<User?> AddAsync(User user)
        {
            var existingUser = await _context.User
                .FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower());

            if (existingUser != null)
                return null;

            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsActive = true;

            // Set default role (assuming you have a default employee role)
            var employeeRole = await _context.Role.FirstOrDefaultAsync(r => r.RoleName == "Employee");
            if (employeeRole != null)
            {
                user.RoleId = employeeRole.RoleId;
            }
            else
            {
                _logger.LogError("Employee role not found in database");
                return null;
            }

            await _context.User.AddAsync(user);
            await SaveChangesAsync();

            return user;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    _logger.LogWarning("Login attempt with empty email or password");
                    return null;
                }

                var user = await _context.User
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning($"Login attempt failed: User not found or inactive - {request.Email}");
                    return null;
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning($"Login attempt failed: Invalid password for user {request.Email}");
                    return null;
                }

                if (user.Role == null)
                {
                    _logger.LogError($"User {request.Email} has no assigned role");
                    return null;
                }

                var roles = new List<string> { user.Role.RoleName };
                var token = await _tokenRepository.CreateTokenAsync(user, roles);

                // Update last login
                user.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(user);

                return new LoginResponseDto
                {
                    Email = user.Email,
                    Roles = roles,
                    Token = token
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login process: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Entry(user).State = EntityState.Modified;
            return await SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                return false;

            _context.User.Remove(user);
            return await SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.User.AnyAsync(u => u.UserId == id);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        public async Task<Guid> GetUserIdByEmailAsync(string email)
        {
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Email == email);

            return user?.UserId ?? Guid.Empty;
        }
    }
}
