using worksystem.DTOs;
using worksystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace worksystem.Services
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDTO>> GetAllEmployeesByMonth(DateOnly date);
        Task<EmployeeDTO> GetEmployeeById(int EmployeeId);
        Task<List<EmployeeDTO>> GetEmployeesByRole(UserRole role);
        Task<EmployeeDTO> GetEmployeeByUsername(string Username);
        Task<bool> AnyAdminExists();
        Task<EmployeeDTO> CreateEmployee(EmployeeDTO employee);
        Task<EmployeeDTO> UpdateFullNameByEmployeeId(int EmployeeId, EmployeeDTO employee);
        Task<EmployeeDTO> UpdateUsernameByEmployeeId(int EmployeeId, EmployeeDTO employee);
        Task<EmployeeDTO> UpdatePasswordByEmployeeId(int EmployeeId, EmployeeDTO employee);
        Task<IActionResult> UpdatePassword([FromBody] PasswordDTO password);
        Task<EmployeeDTO> UpdateUserRoleByEmployeeId(int EmployeeId, EmployeeDTO employee);
        Task DeleteEmployee(int EmployeeId, EmployeeDTO employee);
    }
}
