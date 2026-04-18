using System;

namespace NFe.Domain.Entities
{
    public class User
    {
        public int id { get; set; }
        public string username { get; set; } = "";
        public string email { get; set; } = "";
        public string password_hash { get; set; } = "";
        public string role { get; set; } = "User";
        public bool is_active { get; set; } = true;
        public DateTime create_at { get; set; } = DateTime.UtcNow;
        public DateTime update_at { get; set; } = DateTime.UtcNow;
    }
}