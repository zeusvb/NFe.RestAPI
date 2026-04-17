using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCryptNet = BCrypt.Net.BCrypt;
using NFe.Application.DTOs.Auth;
using NFe.Application.Interfaces;
using NFe.Infrastructure.Data;

namespace NFe.Application.Services
{
    public class AuthService : NFe.Application.Interfaces.IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly NfeDbContext _dbContext;

        public AuthService(IConfiguration configuration, NfeDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Usuário ou senha inválido");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Usuário inativo");

            var token = GenerateJwtToken(user);

            return new LoginResponse
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
            };
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var key = GetJwtSecretKeyBytes();
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
            var key = GetJwtSecretKeyBytes();
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

        private bool VerifyPassword(string password, string hash)
        {
            return BCryptNet.Verify(password, hash);
        }

        private byte[] GetJwtSecretKeyBytes()
        {
            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("Configuração Jwt:SecretKey não encontrada.");
            }

            return Encoding.ASCII.GetBytes(secretKey);
        }
    }
}
