using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CycleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepository;
        private readonly ITokenRepository tokenRepository;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthRepository authRepository, 
            ITokenRepository tokenRepository,
            ILogger<AuthController> logger)
        {
            this.authRepository = authRepository;
            this.tokenRepository = tokenRepository;
            this._logger = logger;
        }

        [HttpPost("register/Employee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterRequestDto req)
        {
            try 
            {
                // Validate request
                if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password) || 
                    string.IsNullOrEmpty(req.FirstName) || string.IsNullOrEmpty(req.LastName))
                {
                    _logger.LogWarning("Registration failed: Required fields missing");
                    return BadRequest(new { Success = false, Message = "Email, password, first name, and last name are required" });
                }
                
                var user = new User
                {
                    UserId = Guid.NewGuid(), // Explicitly assign a new GUID
                    Email = req.Email,
                    FirstName = req.FirstName,
                    LastName = req.LastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                    PhoneNumber = req.PhoneNumber,
                    Address = req.Address
                };
                
                _logger.LogInformation($"Attempting to register employee with email {req.Email}");
                var response = await authRepository.AddAsync(user);

                if (!response.Success)
                {
                    _logger.LogWarning($"Registration failed: {response.Message}");
                    return BadRequest(response);
                }
                
                _logger.LogInformation($"Employee registered successfully with email {req.Email} and ID {response.UserId}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during employee registration: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                
                return StatusCode(500, new { Success = false, Message = "An error occurred during registration." });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto req)
        {
            try
            {
                _logger.LogInformation($"Login attempt for email: {req.Email}");

                if (!ModelState.IsValid)
                {
                    return ValidationProblem(ModelState);
                }

                var loginResponse = await authRepository.LoginAsync(req);

                if (loginResponse == null)
                {
                    _logger.LogWarning($"Login failed for email: {req.Email}");
                    ModelState.AddModelError("", "Email or password is incorrect.");
                    return ValidationProblem(ModelState);
                }

                // Get the user ID from the repository
                var userId = await authRepository.GetUserIdByEmailAsync(req.Email);
                if (userId != Guid.Empty)
                {
                    loginResponse.UserId = userId;
                }

                _logger.LogInformation($"Login successful for email: {req.Email} with userId: {userId}");
                return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return StatusCode(500, "An error occurred during login.");
            }
        }
    }
}
