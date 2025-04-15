using worksystem.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace worksystem.Services
{
    public interface IWorklogService
    {
        Task<List<WorklogDTO>> GetAllWorklogsByMonth(DateOnly Date);
        Task<List<WorklogDTO>> GetWorklogsByEmployeeId(int EmployeeId, DateOnly month);
        Task<List<WorklogDTO>> GetWorklogsByDate(DateOnly Date);
    }
}