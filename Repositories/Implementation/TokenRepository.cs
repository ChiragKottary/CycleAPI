using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Repositories.Interface;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CycleAPI.Repositories.Implementation
{
    public class TokenRepository : ITokenRepository
    {
        private readonly IConfiguration _configuration;
        private readonly HashSet<string> _revokedTokens;

        public TokenRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _revokedTokens = new HashSet<string>();
        }

        public async Task<string> CreateTokenAsync(User user, List<string> roles)
        {
            return await Task.Run(() =>
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Username)
                };

                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiryInHours"])),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            });
        }

        public async Task<string> CreateCustomerTokenAsync(CustomerDto customer)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured")));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                new(ClaimTypes.Email, customer.Email),
                new(ClaimTypes.Name, customer.FullName),
                new(ClaimTypes.Role, "Customer")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer not configured"),
                audience: _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience not configured"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            return await Task.Run(() =>
            {
                if (_revokedTokens.Contains(token))
                    return false;

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

                    return validatedToken != null;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(token))
                    return false;

                _revokedTokens.Add(token);
                return true;
            });
        }
    }
}
