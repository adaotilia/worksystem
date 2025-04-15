using worksystem.Models;

namespace worksystem.DTOs
{
    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class RegisterRequest
    {
        public required string FullName { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required UserRole UserRole { get; set; }
    }
}