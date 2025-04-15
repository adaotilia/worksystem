using worksystem.Data;
using worksystem.DTOs;
using worksystem.Models;
using worksystem.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace worksystem.Services
{
    public class WorklogService : IWorklogService
    {
        private readonly AppDbContext _context;
    
        public WorklogService(AppDbContext context)
        {
            _context = context;
        }
        //Az összes beosztás egy hónapra vonatkoztatva.
        public async Task<List<WorklogDTO>> GetAllWorklogsByMonth(DateOnly month)
        {
            var startOfMonth = new DateOnly(month.Year, month.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            
            var employeeIds = await _context.Employees.Select(e => e.EmployeeId).ToListAsync();
            var monthlyreportService = new MonthlyreportService(_context);
            var scheduleService = new ScheduleService(_context);
            foreach (var employeeId in employeeIds)
            {
                await monthlyreportService.UpdateOrCreateMonthlyReportsPerDay(employeeId, month.Year, month.Month);
                await scheduleService.GetSchedulesByEmployeeId(employeeId);
            }

            var monthlyReports = await _context.Monthlyreports
                .Where(mr => mr.ReportMonth.Year == month.Year && mr.ReportMonth.Month == month.Month)
                .Include(mr => mr.Employee)
                .ToListAsync();

            var schedules = await _context.Schedules
                .Where(s => s.ScheduledDate >= startOfMonth && s.ScheduledDate <= endOfMonth)
                .Include(s => s.Employee)
                .ToListAsync();

            var existingWorklogs = await _context.Worklogs
                .Where(wl => wl.ScheduledDate >= startOfMonth && wl.ScheduledDate <= endOfMonth)
                .ToListAsync();
            _context.Worklogs.RemoveRange(existingWorklogs);

            var newWorklogs = schedules.Select(s => {
                var mr = monthlyReports.FirstOrDefault(m => m.EmployeeId == s.EmployeeId && m.Date == s.ScheduledDate);
                var monthlySummary = monthlyReports.FirstOrDefault(m => m.EmployeeId == s.EmployeeId && m.Date == startOfMonth);
                return new Worklog
                {
                    EmployeeId = s.EmployeeId,
                    Employee = s.Employee,
                    ScheduledDate = s.ScheduledDate,
                    WorkHours = mr?.WorkHours != null ? (int?)mr.WorkHours : 0,
                    OvertimeHours = mr?.OvertimeHours != null ? (int?)mr.OvertimeHours : 0,
                    ScheduledHours = (int?)ScheduleCalculator.CalculateHours(s.StartTime, s.EndTime),
                    Type = (int)s.Type,
                    MonthlyWorkDays = monthlySummary?.MonthlyWorkDays ?? 0,
                    ScheduledWorkDays = (int?)s.ScheduledWorkDays,
                    MonthlyOvertimeHours = monthlySummary?.MonthlyOvertimeHours != null ? (int?)monthlySummary.MonthlyOvertimeHours : 0
                };
            }).ToList();

            await _context.Worklogs.AddRangeAsync(newWorklogs);
            await _context.SaveChangesAsync();

            return newWorklogs.Select(wl => new WorklogDTO
            {
                EmployeeId = wl.EmployeeId,
                FullName = wl.Employee.FullName,
                ScheduledDate = wl.ScheduledDate,
                WorkHours = wl.WorkHours ?? 0,
                OvertimeHours = wl.OvertimeHours ?? 0,
                ScheduledHours = wl.ScheduledHours ?? 0,
                Type = (ScheduleType)wl.Type,
                MonthlyWorkDays = wl.MonthlyWorkDays ?? 0,
                ScheduledWorkDays = wl.ScheduledWorkDays ?? 0,
                MonthlyOvertimeHours = wl.MonthlyOvertimeHours ?? 0
            }).ToList();
        }

        //A megadott Id alapján beosztás egy hónapra vonatkoztatva.
       public async Task<List<WorklogDTO>> GetWorklogsByEmployeeId(int EmployeeId, DateOnly month)
        {
            var startOfMonth = new DateOnly(month.Year, month.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var monthlyreportService = new MonthlyreportService(_context);
            var scheduleService = new ScheduleService(_context);
            await monthlyreportService.UpdateOrCreateMonthlyReportsPerDay(EmployeeId, month.Year, month.Month);
            await scheduleService.GetSchedulesByEmployeeId(EmployeeId);

            var monthlyReport = await _context.Monthlyreports
                .FirstOrDefaultAsync(mr => 
                    mr.EmployeeId == EmployeeId && 
                    mr.ReportMonth.Year == month.Year && mr.ReportMonth.Month == month.Month);

            var schedules = await _context.Schedules
                .Where(s => 
                    s.EmployeeId == EmployeeId &&
                    s.ScheduledDate >= startOfMonth &&
                    s.ScheduledDate <= endOfMonth)
                .Include(s => s.Employee)
                .ToListAsync();

            var existingWorklogs = await _context.Worklogs
                .Where(wl => wl.EmployeeId == EmployeeId && wl.ScheduledDate >= startOfMonth && wl.ScheduledDate <= endOfMonth)
                .ToListAsync();
            _context.Worklogs.RemoveRange(existingWorklogs);

            var newWorklogs = schedules.Select(s => new Worklog
            {
                EmployeeId = s.EmployeeId,
                Employee = s.Employee,
                ScheduledDate = s.ScheduledDate,
                WorkHours = monthlyReport?.WorkHours != null ? (int?)monthlyReport.WorkHours : 0,
                OvertimeHours = monthlyReport?.OvertimeHours != null ? (int?)monthlyReport.OvertimeHours : 0,
                ScheduledHours = (int?)ScheduleCalculator.CalculateHours(s.StartTime, s.EndTime),
                Type = (int)s.Type,
                MonthlyWorkDays = monthlyReport?.MonthlyWorkDays ?? 0,
                ScheduledWorkDays = (int?)s.ScheduledWorkDays,
                MonthlyOvertimeHours = monthlyReport?.MonthlyOvertimeHours != null ? (int?)monthlyReport.MonthlyOvertimeHours : 0
            }).ToList();

            await _context.Worklogs.AddRangeAsync(newWorklogs);
            await _context.SaveChangesAsync();

            return newWorklogs.Select(wl => new WorklogDTO
            {
                EmployeeId = wl.EmployeeId,
                FullName = wl.Employee.FullName,
                ScheduledDate = wl.ScheduledDate,
                WorkHours = wl.WorkHours ?? 0,
                OvertimeHours = wl.OvertimeHours ?? 0,
                ScheduledHours = wl.ScheduledHours ?? 0,
                Type = (ScheduleType)wl.Type,
                MonthlyWorkDays = wl.MonthlyWorkDays ?? 0,
                ScheduledWorkDays = wl.ScheduledWorkDays ?? 0,
                MonthlyOvertimeHours = wl.MonthlyOvertimeHours ?? 0
            }).ToList();
        }
        //A megadott Id alapján beosztás egy adott dátumra vonatkoztatva.
        public async Task<List<WorklogDTO>> GetWorklogsByDate(DateOnly date)
        {
            var month = new DateOnly(date.Year, date.Month, 1);

            var employeeIds = await _context.Employees.Select(e => e.EmployeeId).ToListAsync();
            var monthlyreportService = new MonthlyreportService(_context);
            var scheduleService = new ScheduleService(_context);
            foreach (var employeeId in employeeIds)
            {
                await monthlyreportService.UpdateOrCreateMonthlyReportsPerDay(employeeId, date.Year, date.Month);
                await scheduleService.GetSchedulesByEmployeeId(employeeId);
            }

            var monthlyReports = await _context.Monthlyreports
                .Where(mr => mr.ReportMonth.Year == month.Year && mr.ReportMonth.Month == month.Month)
                .Include(mr => mr.Employee)
                .ToListAsync();

            var schedules = await _context.Schedules
                .Where(s => s.ScheduledDate == date)
                .Include(s => s.Employee)
                .ToListAsync();

            var existingWorklogs = await _context.Worklogs
                .Where(wl => wl.ScheduledDate == date)
                .ToListAsync();
            _context.Worklogs.RemoveRange(existingWorklogs);

            var newWorklogs = schedules.Select(s => {
                var mr = monthlyReports.FirstOrDefault(m => m.EmployeeId == s.EmployeeId && m.Date == s.ScheduledDate);
                var monthlySummary = monthlyReports.FirstOrDefault(m => m.EmployeeId == s.EmployeeId && m.Date == month);
                return new Worklog
                {
                    EmployeeId = s.EmployeeId,
                    Employee = s.Employee,
                    ScheduledDate = s.ScheduledDate,
                    WorkHours = mr?.WorkHours != null ? (int?)mr.WorkHours : 0,
                    OvertimeHours = mr?.OvertimeHours != null ? (int?)mr.OvertimeHours : 0,
                    ScheduledHours = (int?)ScheduleCalculator.CalculateHours(s.StartTime, s.EndTime),
                    Type = (int)s.Type,
                    MonthlyWorkDays = monthlySummary?.MonthlyWorkDays ?? 0,
                    ScheduledWorkDays = (int?)s.ScheduledWorkDays,
                    MonthlyOvertimeHours = monthlySummary?.MonthlyOvertimeHours != null ? (int?)monthlySummary.MonthlyOvertimeHours : 0
                };
            }).ToList();

            await _context.Worklogs.AddRangeAsync(newWorklogs);
            await _context.SaveChangesAsync();

            return newWorklogs.Select(wl => new WorklogDTO
            {
                EmployeeId = wl.EmployeeId,
                FullName = wl.Employee.FullName,
                ScheduledDate = wl.ScheduledDate,
                WorkHours = wl.WorkHours ?? 0,
                OvertimeHours = wl.OvertimeHours ?? 0,
                ScheduledHours = wl.ScheduledHours ?? 0,
                Type = (ScheduleType)wl.Type,
                MonthlyWorkDays = wl.MonthlyWorkDays ?? 0,
                ScheduledWorkDays = wl.ScheduledWorkDays ?? 0,
                MonthlyOvertimeHours = wl.MonthlyOvertimeHours ?? 0
            }).ToList();
        }
    }
}