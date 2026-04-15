using System.ComponentModel.DataAnnotations;

namespace NFe.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username é obrigatório")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password é obrigatório")]
        public string Password { get; set; }
    }
}