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

        public async Task<RegisterResponseDto> AddAsync(User user)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.User
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower());

                if (existingUser != null)
                {
                    _logger.LogWarning($"User with email {user.Email} already exists");
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "User with this email already exists",
                        Email = user.Email
                    };
                }

                // Set basic user properties
                if (user.UserId == Guid.Empty)
                {
                    user.UserId = Guid.NewGuid();
                }
                
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                user.IsActive = true;
                user.Username = user.Email.ToLower();
                
                // Log role assignment attempt
                _logger.LogInformation("Attempting to assign Employee role to new user");
                
                // Find employee role
                var employeeRole = await _context.Role.FirstOrDefaultAsync(r => r.RoleName == "Employee");
                if (employeeRole == null)
                {
                    // Check if any roles exist at all
                    var roleCount = await _context.Role.CountAsync();
                    if (roleCount == 0)
                    {
                        _logger.LogCritical("No roles found in database. Database may not be properly initialized.");
                        return new RegisterResponseDto
                        {
                            Success = false,
                            Message = "System configuration error: No roles available in the system."
                        };
                    }
                    
                    _logger.LogError("Employee role not found in database. Available roles:");
                    var availableRoles = await _context.Role.ToListAsync();
                    foreach (var role in availableRoles)
                    {
                        _logger.LogInformation($"Role: {role.RoleName}, ID: {role.RoleId}");
                    }
                    
                    // Fall back to first available role if no "Employee" role
                    employeeRole = availableRoles.FirstOrDefault();
                    if (employeeRole != null)
                    {
                        _logger.LogWarning($"Falling back to role: {employeeRole.RoleName}");
                        user.RoleId = employeeRole.RoleId;
                    }
                    else
                    {
                        return new RegisterResponseDto
                        {
                            Success = false,
                            Message = "Default role not found. Registration failed."
                        };
                    }
                }
                else
                {
                    user.RoleId = employeeRole.RoleId;
                    _logger.LogInformation($"Assigned role '{employeeRole.RoleName}' with ID: {employeeRole.RoleId}");
                }

                // Add user to database
                await _context.User.AddAsync(user);
                var saveResult = await SaveChangesAsync();

                if (!saveResult)
                {
                    _logger.LogError("Failed to save user to database. No rows affected.");
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "Failed to save user to database"
                    };
                }

                _logger.LogInformation($"User registered successfully: ID={user.UserId}, Email={user.Email}");
                return new RegisterResponseDto
                {
                    Success = true,
                    Message = "User registered successfully",
                    UserId = user.UserId,
                    Email = user.Email,
                    Role = employeeRole.RoleName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error registering user: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = $"An error occurred during registration: {ex.Message}"
                };
            }
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

        public async Task<IEnumerable<User>> GetAllEmployeesAsync()
        {
            return await _context.User
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<bool> DeactivateEmployeeAsync(Guid id)
        {
            var employee = await GetByIdAsync(id);
            if (employee == null)
                return false;

            employee.IsActive = false;
            employee.UpdatedAt = DateTime.UtcNow;
            _context.Entry(employee).State = EntityState.Modified;
            return await SaveChangesAsync();
        }

        public async Task<bool> ActivateEmployeeAsync(Guid id)
        {
            var employee = await GetByIdAsync(id);
            if (employee == null)
                return false;

            employee.IsActive = true;
            employee.UpdatedAt = DateTime.UtcNow;
            _context.Entry(employee).State = EntityState.Modified;
            return await SaveChangesAsync();
        }

        public async Task<bool> RoleExistsAsync(Guid roleId)
        {
            return await _context.Role.AnyAsync(r => r.RoleId == roleId);
        }
    }
}
