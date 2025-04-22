using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CycleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepository;
        private readonly ITokenRepository tokenRepository;

        public AuthController(IAuthRepository authRepository, ITokenRepository tokenRepository)
        {
            this.authRepository = authRepository;
            this.tokenRepository = tokenRepository;
        }


        [HttpPost("register/Employee")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterRequestDto req)
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
                ModelState.AddModelError("Email", "User already exists with this email.");
                return ValidationProblem(ModelState);
            }
            
            return Ok("Employee registered successfully." + exist);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto req)
        {

            var loginResponse = await authRepository.LoginAsync(req);

            if (loginResponse == null)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return ValidationProblem(ModelState);
            }

            return Ok(loginResponse);
        }
    }

}
