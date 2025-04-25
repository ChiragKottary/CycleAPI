using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Repositories.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CycleAPI.Repositories.Implementation
{
    public class TokenRepository : ITokenRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenRepository> _logger;
        private readonly HashSet<string> _revokedTokens;

        public TokenRepository(
            IConfiguration configuration,
            ILogger<TokenRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _revokedTokens = new HashSet<string>();

            // Validate JWT configuration on startup
            if (string.IsNullOrEmpty(_configuration["Jwt:Key"]))
                throw new InvalidOperationException("JWT Key is not configured");
            if (string.IsNullOrEmpty(_configuration["Jwt:Issuer"]))
                throw new InvalidOperationException("JWT Issuer is not configured");
            if (string.IsNullOrEmpty(_configuration["Jwt:Audience"]))
                throw new InvalidOperationException("JWT Audience is not configured");
        }

        public async Task<string> CreateTokenAsync(User user, List<string> roles)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
                };

                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(24),
                    signingCredentials: credentials
                );

                return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating token for user {user.Email}: {ex.Message}");
                throw;
            }
        }

        public async Task<string> CreateCustomerTokenAsync(CustomerDto customer)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                    new Claim(ClaimTypes.Email, customer.Email),
                    new Claim(ClaimTypes.Name, $"{customer.FirstName} {customer.LastName}"),
                    new Claim(ClaimTypes.Role, "Customer")
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation($"Successfully created token for customer {customer.Email}");
                return await Task.FromResult(tokenString);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating token for customer {customer.Email}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (_revokedTokens.Contains(token))
            {
                _logger.LogWarning("Attempt to validate revoked token");
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return await Task.FromResult(validatedToken != null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token validation failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Attempt to revoke null or empty token");
                return false;
            }

            _revokedTokens.Add(token);
            _logger.LogInformation("Token successfully revoked");
            return await Task.FromResult(true);
        }
    }
}
