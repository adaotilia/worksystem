using worksystem.Models;
using System.Security.Claims;

namespace worksystem.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(Employee employee);
        int? ValidateToken(string token);
    }
}