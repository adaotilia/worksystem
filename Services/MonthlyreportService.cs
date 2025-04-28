using System.Collections.Generic;
using System.Threading.Tasks;
using worksystem.DTOs;
using worksystem.Models;
using worksystem.Data;
using Microsoft.EntityFrameworkCore;
using worksystem.Helpers;

namespace worksystem.Services
{
    public class MonthlyreportService : IMonthlyreportService
    {
        private readonly AppDbContext _context;

        public MonthlyreportService(AppDbContext context)
        {
            _context = context;
        }

        //Összes havi kimutatás listázása.
        public async Task<List<MonthlyreportDTO>> GetAllMonthlyreportsByReportMonth(DateOnly ReportMonth)
        {
            var employeeIds = await _context.Employees.Select(e => e.EmployeeId).ToListAsync();
            foreach (var employeeId in employeeIds)
            {
                await UpdateOrCreateMonthlyReportsPerDay(employeeId, ReportMonth.Year, ReportMonth.Month);
            }

            var monthlyReports = await _context.Monthlyreports
                .Include(m => m.Employee)
                .Where(m => m.ReportMonth == ReportMonth)
                .ToListAsync();

            return monthlyReports.Select(report => new MonthlyreportDTO
            {
                MonthlyreportId = report.MonthlyreportId,
                EmployeeId = report.EmployeeId,
                FullName = report.Employee.FullName,
                ReportMonth = report.ReportMonth,
                Date = report.Date,
                WorkHours = report.WorkHours,
                OvertimeHours = report.OvertimeHours,
                MonthlyWorkDays = report.MonthlyWorkDays,
                MonthlyWorkHours = report.MonthlyWorkHours,
                MonthlyOvertimeHours = report.MonthlyOvertimeHours
            }).ToList();
        }

        //Egy dolgozó kiválasztott havi kimutatása EmployeeId alapján.
        public async Task<List<MonthlyreportDTO>> GetMonthlyreportsByEmployeeId(int EmployeeId)
        {
            var months = await _context.Schedules
                .Where(s => s.EmployeeId == EmployeeId)
                .Select(s => new { s.ScheduledDate.Year, s.ScheduledDate.Month })
                .Distinct()
                .ToListAsync();

            foreach (var m in months)
            {
                await UpdateOrCreateMonthlyReportsPerDay(EmployeeId, m.Year, m.Month);
            }

            var monthlyReports = await _context.Monthlyreports
                .Include(m => m.Employee)
                .Where(m => m.EmployeeId == EmployeeId)
                .ToListAsync();

            return monthlyReports.Select(report => new MonthlyreportDTO
            {
                MonthlyreportId = report.MonthlyreportId,
                EmployeeId = report.EmployeeId,
                FullName = report.Employee.FullName,
                ReportMonth = report.ReportMonth,
                Date = report.Date,
                WorkHours = report.WorkHours,
                OvertimeHours = report.OvertimeHours,
                MonthlyWorkDays = report.MonthlyWorkDays,
                MonthlyWorkHours = report.MonthlyWorkHours,
                MonthlyOvertimeHours = report.MonthlyOvertimeHours
            }).ToList();
        }

        // Frissíti vagy újragenerálja egy dolgozó adott havi riportját a checkpointok és beosztások alapján
        public async Task<List<Monthlyreport>> UpdateOrCreateMonthlyReportsPerDay(int employeeId, int year, int month)
        {
            var reportMonth = new DateOnly(year, month, 1);
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
            if (employee == null) throw new InvalidOperationException("Dolgozó nem található.");

            var checkpoints = await _context.Checkpoints
                .Where(c => c.EmployeeId == employeeId && c.CheckInTime.HasValue && c.CheckOutTime.HasValue &&
                    c.CheckInTime.Value.Year == year && c.CheckInTime.Value.Month == month)
                .ToListAsync();
            var checkpointDtos = checkpoints.Select(c => new CheckpointDTO {
                CheckpointId = c.CheckpointId,
                EmployeeId = c.EmployeeId,
                CheckInTime = c.CheckInTime,
                CheckOutTime = c.CheckOutTime,
                SessionStatus = c.SessionStatus
            }).ToList();

            var days = checkpointDtos
                .Where(cp => cp.CheckInTime.HasValue && cp.CheckOutTime.HasValue)
                .GroupBy(cp => cp.CheckInTime.Value.Date)
                .ToList();

            var reports = new List<Monthlyreport>();
            decimal monthlyOvertimeSum = 0;
            foreach (var dayGroup in days)
            {
                var day = DateOnly.FromDateTime(dayGroup.Key);
                var workHours = dayGroup.Sum(cp => (decimal)(cp.CheckOutTime.Value - cp.CheckInTime.Value).TotalHours);
                var workDay = workHours > 0 ? 1 : 0;
                var schedules = await _context.Schedules
                    .Where(s => s.EmployeeId == employeeId && s.ScheduledDate == day)
                    .ToListAsync();
                var scheduledHours = schedules.Sum(s => worksystem.Helpers.ScheduleCalculator.CalculateHours(s.StartTime, s.EndTime));
                var overtime = worksystem.Helpers.OvertimeHoursCalculator.CalculateMonthlyOvertimeHours(workHours, scheduledHours);
                monthlyOvertimeSum += overtime;

                var monthlyReport = await _context.Monthlyreports.FirstOrDefaultAsync(mr => mr.EmployeeId == employeeId && mr.Date == day);
                if (monthlyReport == null)
                {
                    monthlyReport = new Monthlyreport
                    {
                        EmployeeId = employeeId,
                        Employee = employee,
                        ReportMonth = reportMonth,
                        Date = day
                    };
                    _context.Monthlyreports.Add(monthlyReport);
                }
                monthlyReport.WorkHours = workHours;
                monthlyReport.MonthlyWorkDays = workDay;
                monthlyReport.MonthlyWorkHours = workHours;
                monthlyReport.OvertimeHours = overtime;

                reports.Add(monthlyReport);
            }

            var monthlySummary = await _context.Monthlyreports.FirstOrDefaultAsync(mr => mr.EmployeeId == employeeId && mr.Date == reportMonth);
            if (monthlySummary == null)
            {
                monthlySummary = new Monthlyreport
                {
                    EmployeeId = employeeId,
                    Employee = employee,
                    ReportMonth = reportMonth,
                    Date = reportMonth
                };
                _context.Monthlyreports.Add(monthlySummary);
            }
            monthlySummary.MonthlyOvertimeHours = monthlyOvertimeSum;
            monthlySummary.MonthlyWorkDays = reports.Sum(r => r.MonthlyWorkDays);

            await _context.SaveChangesAsync();
            return reports;
        }
    }
}