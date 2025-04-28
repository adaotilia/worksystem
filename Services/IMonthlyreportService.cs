using worksystem.DTOs;
using worksystem.Models;

namespace worksystem.Services
{
    public interface IMonthlyreportService
    {
        Task<List<MonthlyreportDTO>> GetAllMonthlyreportsByReportMonth(DateOnly ReportMonth);
        Task<List<MonthlyreportDTO>> GetMonthlyreportsByEmployeeId(int EmployeeId);
        Task<List<Monthlyreport>> UpdateOrCreateMonthlyReportsPerDay(int employeeId, int year, int month);
    }
}
