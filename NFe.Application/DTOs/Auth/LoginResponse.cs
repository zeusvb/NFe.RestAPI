namespace NFe.Application.DTOs.Auth
{
    public class LoginResponse
    {
        public required string Token { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public int id { get; set; }
        public string username { get; set; } = "";
        public string email { get; set; } = "";
        public string role { get; set; } = "";
    }
}