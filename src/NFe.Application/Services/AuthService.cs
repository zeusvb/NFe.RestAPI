using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NFe.Application.DTOs.Auth;
using NFe.Application.Interfaces;

namespace NFe.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var configuredUsername = _configuration["Auth:Username"];
            var configuredPassword = _configuration["Auth:Password"];

            if (string.IsNullOrWhiteSpace(configuredUsername) || string.IsNullOrWhiteSpace(configuredPassword))
                throw new InvalidOperationException("Credenciais de autenticação não configuradas.");

            var requestPassword = request.Password ?? string.Empty;
            var configuredPasswordBytes = Encoding.UTF8.GetBytes(configuredPassword);
            var requestPasswordBytes = Encoding.UTF8.GetBytes(requestPassword);
            var passwordMatches = configuredPasswordBytes.Length == requestPasswordBytes.Length &&
                                  CryptographicOperations.FixedTimeEquals(configuredPasswordBytes, requestPasswordBytes);

            if (!string.Equals(request.Username, configuredUsername, StringComparison.OrdinalIgnoreCase) ||
                !passwordMatches)
                throw new UnauthorizedAccessException("Usuário ou senha inválido");

            var user = new NFe.Domain.Entities.User
            {
                Id = 1,
                Username = configuredUsername,
                Email = $"{configuredUsername}@nfe.local",
                Role = "Admin",
                IsActive = true,
                PasswordHash = string.Empty
            };

            var token = GenerateJwtToken(user);

            return Task.FromResult(new LoginResponse
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60") * 60,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                }
            });
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
                var handler = new JwtSecurityTokenHandler();

                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private string GenerateJwtToken(NFe.Domain.Entities.User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
            var handler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60")),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

    }
}
