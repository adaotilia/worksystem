using worksystem.DTOs;
using worksystem.Models;
using worksystem.Data;

namespace worksystem.Services
{
    public interface IScheduleService
    {
        Task<List<ScheduleDTO>> GetAllSchedulesByMonth(DateOnly Date);
        Task<List<ScheduleDTO>> GetSchedulesByEmployeeId(int EmployeeId);
        Task<List<ScheduleDTO>> GetSchedulesByDate(DateOnly Date);
        Task<ScheduleDTO> CreateSchedule(int EmployeeId, ScheduleDTO schedule);
        Task<ScheduleDTO> UpdateSchedule(int EmployeeId, DateOnly month, ScheduleDTO schedule);
        Task DeleteSchedule(int EmployeeId, DateOnly month);
    }
}