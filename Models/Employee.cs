using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace worksystem.Models
{
    public enum UserRole
    {
        Employee,
        Manager,
        Admin
    }


    public class Employee
    {
    [Key]
    [Range(100, 999, ErrorMessage = "Az ID-nak 100 és 999 között kell lennie.")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "Név megadása kötelező.")]
    public required string FullName { get; set; }

    [Required(ErrorMessage = "Felhasználónév megadása kötelező.")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Jelszó megadása kötelező.")]
    public required string PasswordHash { get; set; }

    [Required(ErrorMessage = "Jogosultság meghatározása kötelező.")]
    public required UserRole UserRole { get; set; }
    }
}