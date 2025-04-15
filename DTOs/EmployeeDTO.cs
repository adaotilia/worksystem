using System;
using worksystem.Models;
using System.Text.Json.Serialization;

namespace worksystem.DTOs
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public int EmployeeId { get => Id; set => Id = value; }
        public required string FullName { get; set; }
        public required string Username { get; set; }
        public string? NewUsername { get; set; }
        public string? NewFullName { get; set; }
        public string? Password { get; set; }
        [JsonIgnore]
        public string? PasswordHash { get; set; }
        public required UserRole UserRole { get; set; }
    }
}