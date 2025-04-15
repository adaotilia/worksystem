namespace worksystem.DTOs
{
    public class PasswordDTO
    {
        public int EmployeeId { get; set; }
        public string? Password { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
    }
}
