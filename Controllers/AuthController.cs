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
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterRequestDto req)
        {
            try 
            {
                var user = new User
                {
                    Email = req.Email,
                    FirstName = req.FirstName,
                    LastName = req.LastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                };
                var exist = await authRepository.AddAsync(user);

                if (exist == null)
                {
                    _logger.LogWarning($"Registration failed: User already exists with email {req.Email}");
                    ModelState.AddModelError("Email", "User already exists with this email.");
                    return ValidationProblem(ModelState);
                }
                
                _logger.LogInformation($"Employee registered successfully with email {req.Email}");
                return Ok("Employee registered successfully." + exist);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during employee registration: {ex.Message}");
                return StatusCode(500, "An error occurred during registration.");
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
