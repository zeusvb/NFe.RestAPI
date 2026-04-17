using System.Threading.Tasks;
using NFe.Application.DTOs.Auth;

namespace NFe.Application.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<bool> ValidateTokenAsync(string token);
    }
}