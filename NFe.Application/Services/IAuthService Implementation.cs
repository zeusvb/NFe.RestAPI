using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using NFe.Application.DTOs.Auth;
using NFe.Application.Interfaces;
using NFe.Infrastructure.Data;
using NFe.Domain.Entities;

namespace NFe.Application.Services
{
    public class AuthService : NFe.Application.Interfaces.IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly NfeDbContext _dbContext;

        private const string JwtSecretKeyConfig = "Jwt:SecretKey";
        private const string JwtIssuerConfig = "Jwt:Issuer";
        private const string JwtAudienceConfig = "Jwt:Audience";
        private const string JwtExpirationMinutesConfig = "Jwt:ExpirationMinutes";
        private const int DefaultExpirationMinutes = 60;

        public AuthService(IConfiguration configuration, NfeDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.username.ToLower() == request.Username.ToLower());

            if (user == null || !VerifyPassword(request.Password, user.password_hash))
                throw new UnauthorizedAccessException("Usuário ou senha inválido");

            if (!user.is_active)
                throw new UnauthorizedAccessException("Usuário inativo");

            var token = GenerateJwtToken(user);

            return new LoginResponse
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = int.Parse(_configuration[JwtExpirationMinutesConfig] ?? DefaultExpirationMinutes.ToString()) * 60,
                User = new UserDto
                {
                    id = user.id,
                    username = user.username,
                    email = user.email,
                    role = user.role
                }
            };
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var key = Encoding.ASCII.GetBytes(_configuration[JwtSecretKeyConfig]);
                var handler = new JwtSecurityTokenHandler();

                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration[JwtIssuerConfig],
                    ValidateAudience = true,
                    ValidAudience = _configuration[JwtAudienceConfig],
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
            var key = Encoding.ASCII.GetBytes(_configuration[JwtSecretKeyConfig]);
            var handler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.username),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Role, user.role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration[JwtExpirationMinutesConfig] ?? DefaultExpirationMinutes.ToString())),
                Issuer = _configuration[JwtIssuerConfig],
                Audience = _configuration[JwtAudienceConfig],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        private bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            try
            {
                // Tenta verificar como hash BCrypt
                if (hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$") || hash.StartsWith("$2x$"))
                {
                    return BCrypt.Net.BCrypt.Verify(password, hash);
                }
            }
            catch
            {
                // Se BCrypt falhar, tenta comparação em texto plano (apenas desenvolvimento)
            }

            // Fallback: verificação em texto plano (APENAS para desenvolvimento)
            return password == hash;
        }
    }
}