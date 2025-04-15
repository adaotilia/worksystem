using System.Collections.Generic;
using System.Threading.Tasks;
using worksystem.DTOs;
using worksystem.Models;
using worksystem.Data;
using Microsoft.EntityFrameworkCore;
using worksystem.Helpers;

namespace worksystem.Services
{
    public class ScheduleService : IScheduleService
    { 
         private readonly AppDbContext _context;

        public ScheduleService(AppDbContext context)
        {
            _context = context;
        }
        //A megadott hónaphoz tartozó összes beosztás listázása.
       public async Task<List<ScheduleDTO>> GetAllSchedulesByMonth(DateOnly Date)
        {
            var monthStart = Date;
            var monthEnd = new DateOnly(Date.Year, Date.Month, DateTime.DaysInMonth(Date.Year, Date.Month));

            var schedules = await _context.Schedules
                .Include(s => s.Employee)
                .Where(s => s.ScheduledDate >= monthStart && s.ScheduledDate <= monthEnd)
                .ToListAsync();

            var grouped = schedules.GroupBy(s => new { s.EmployeeId, s.ScheduledDate.Year, s.ScheduledDate.Month });
            foreach (var group in grouped)
            {
                decimal total = ScheduleCalculator.CalculateMonthlyScheduledHours(
                    group, group.Key.Year, group.Key.Month);
                foreach (var schedule in group)
                {
                    schedule.ScheduledMonthlyHours = total;
                    // ScheduledHours újraszámolása mindig a DTO-ba mappelés előtt
                    schedule.ScheduledHours = ScheduleCalculator.CalculateHours(schedule.StartTime, schedule.EndTime);
                }
            }
            await _context.SaveChangesAsync();

            return schedules.Select(schedule => new ScheduleDTO
            {
                ScheduleId = schedule.ScheduleId,
                EmployeeId = schedule.EmployeeId,
                FullName = schedule.Employee.FullName,
                ScheduledDate = schedule.ScheduledDate,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Type = schedule.Type,
                ScheduledHours = ScheduleCalculator.CalculateHours(schedule.StartTime, schedule.EndTime),
                ScheduledMonthlyHours = schedule.ScheduledMonthlyHours,
                ScheduledWorkDays = ScheduleCalculator.CountScheduledShiftsAndOvertimes(new List<Schedule> { schedule })
            }).ToList();
        }
        //A megadott hónaphoz tartozó besoztás lekérése Id alapján.
        public async Task<List<ScheduleDTO>> GetSchedulesByEmployeeId(int EmployeeId)
        {
            var schedules = await _context.Schedules
                .Include(s => s.Employee)
                .Where(s => s.EmployeeId == EmployeeId)
                .ToListAsync();

            var grouped = schedules.GroupBy(s => new { s.EmployeeId, s.ScheduledDate.Year, s.ScheduledDate.Month });
            foreach (var group in grouped)
            {
                decimal total = ScheduleCalculator.CalculateMonthlyScheduledHours(
                    group, group.Key.Year, group.Key.Month);
                foreach (var schedule in group)
                {
                    schedule.ScheduledMonthlyHours = total;
                    schedule.ScheduledHours = ScheduleCalculator.CalculateHours(schedule.StartTime, schedule.EndTime);
                }
            }
            await _context.SaveChangesAsync();

            return schedules.Select(schedule => new ScheduleDTO
            {
                ScheduleId = schedule.ScheduleId,
                EmployeeId = schedule.EmployeeId,
                FullName = schedule.Employee.FullName,
                ScheduledDate = schedule.ScheduledDate,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Type = schedule.Type,
                ScheduledHours = ScheduleCalculator.CalculateHours(schedule.StartTime, schedule.EndTime),
                ScheduledMonthlyHours = schedule.ScheduledMonthlyHours,
                ScheduledWorkDays = ScheduleCalculator.CountScheduledShiftsAndOvertimes(new List<Schedule> { schedule })
            }).ToList();
        }
        //A megadott dátumhoz tartozó összes besoztás lekérése.
        public async Task<List<ScheduleDTO>> GetSchedulesByDate(DateOnly Date)
        {
            var schedules = await _context.Schedules
                .Include(s => s.Employee)
                .Where(s => s.ScheduledDate == Date)
                .ToListAsync();

            var grouped = schedules.GroupBy(s => new { s.EmployeeId, s.ScheduledDate.Year, s.ScheduledDate.Month });
            foreach (var group in grouped)
            {
                decimal total = ScheduleCalculator.CalculateMonthlyScheduledHours(
                    group, group.Key.Year, group.Key.Month);
                foreach (var schedule in group)
                {
                    schedule.ScheduledMonthlyHours = total;
                    schedule.ScheduledHours = ScheduleCalculator.CalculateHours(schedule.StartTime, schedule.EndTime);
                }
            }
            await _context.SaveChangesAsync();

            return schedules.Select(schedule => new ScheduleDTO
            {
                ScheduleId = schedule.ScheduleId,
                EmployeeId = schedule.EmployeeId,
                FullName = schedule.Employee.FullName,
                ScheduledDate = schedule.ScheduledDate,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Type = schedule.Type,
                ScheduledHours = ScheduleCalculator.CalculateHours(schedule.StartTime, schedule.EndTime),
                ScheduledMonthlyHours = schedule.ScheduledMonthlyHours,
                ScheduledWorkDays = ScheduleCalculator.CountScheduledShiftsAndOvertimes(new List<Schedule> { schedule })
            }).ToList();
        }
        //Megadott Id alapján beosztás készítése.
         public async Task<ScheduleDTO> CreateSchedule(int EmployeeId, ScheduleDTO schedule)
        {
            var employee = await _context.Employees.FindAsync(EmployeeId);
            if (employee == null)
            {
                throw new InvalidOperationException("Dolgozó nem található!");
            }

            var newSchedule = new Schedule
            {
                EmployeeId = EmployeeId,
                Employee = employee,
                ScheduledDate = schedule.ScheduledDate, 
                StartTime = schedule.StartTime, 
                EndTime = schedule.EndTime,    
                Type = schedule.Type,
                ScheduledHours = ScheduleCalculator.CalculateHours(schedule.StartTime, schedule.EndTime),
                ScheduledWorkDays = ScheduleCalculator.CountScheduledShiftsAndOvertimes(new List<Schedule> { new Schedule
                {
                    EmployeeId = EmployeeId,
                    Employee = employee,
                    Type = schedule.Type,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime
                } })
            };

            _context.Schedules.Add(newSchedule);
            await _context.SaveChangesAsync();

            return new ScheduleDTO
            {
                ScheduleId = newSchedule.ScheduleId,
                EmployeeId = newSchedule.EmployeeId,
                FullName = newSchedule.Employee.FullName,
                ScheduledDate = newSchedule.ScheduledDate,
                StartTime = newSchedule.StartTime,
                EndTime = newSchedule.EndTime,
                Type = newSchedule.Type,
                ScheduledHours = newSchedule.ScheduledHours,
                ScheduledMonthlyHours = ScheduleCalculator.CalculateMonthlyScheduledHours(
                    _context.Schedules.Where(s => s.EmployeeId == newSchedule.EmployeeId),
                    newSchedule.ScheduledDate.Year,
                    newSchedule.ScheduledDate.Month),
                ScheduledWorkDays = newSchedule.ScheduledWorkDays
            };
        }
        //Beosztás módosítása Id és hónap alapján.
        public async Task<ScheduleDTO> UpdateSchedule(int EmployeeId, DateOnly month, ScheduleDTO schedule)
        {
            var monthStart = new DateOnly(month.Year, month.Month, 1);
            var monthEnd = new DateOnly(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));

            var existingSchedule = await _context.Schedules
                .Where(s => s.EmployeeId == EmployeeId && 
                        s.ScheduledDate >= monthStart && 
                        s.ScheduledDate <= monthEnd)
                .FirstOrDefaultAsync();

            if (existingSchedule == null)
            {
                throw new InvalidOperationException("Beosztás nem található!");
            }

            existingSchedule.StartTime = schedule.StartTime;
            existingSchedule.EndTime = schedule.EndTime;
            existingSchedule.Type = schedule.Type;
            existingSchedule.ScheduledHours = ScheduleCalculator.CalculateHours(schedule.StartTime, schedule.EndTime);
            existingSchedule.ScheduledWorkDays = ScheduleCalculator.CountScheduledShiftsAndOvertimes(new List<Schedule> { existingSchedule });

            await _context.SaveChangesAsync();

            return new ScheduleDTO
            {
                ScheduleId = existingSchedule.ScheduleId,
                EmployeeId = existingSchedule.EmployeeId,
                FullName = existingSchedule.Employee.FullName,
                ScheduledDate = existingSchedule.ScheduledDate,
                StartTime = existingSchedule.StartTime,
                EndTime = existingSchedule.EndTime,
                Type = existingSchedule.Type,
                ScheduledHours = existingSchedule.ScheduledHours,
                ScheduledMonthlyHours = ScheduleCalculator.CalculateMonthlyScheduledHours(
                    _context.Schedules.Where(s => s.EmployeeId == existingSchedule.EmployeeId),
                    existingSchedule.ScheduledDate.Year,
                    existingSchedule.ScheduledDate.Month),
                ScheduledWorkDays = existingSchedule.ScheduledWorkDays
            };
        }
        //Megadott Id és hónap alapján beosztás törlése.
        public async Task DeleteSchedule(int EmployeeId, DateOnly month)
        {
            var monthStart = new DateOnly(month.Year, month.Month, 1);
            var monthEnd = new DateOnly(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));

            var schedule = await _context.Schedules
                .Where(s => s.EmployeeId == EmployeeId && 
                        s.ScheduledDate >= monthStart && 
                        s.ScheduledDate <= monthEnd)
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                throw new InvalidOperationException("Beosztás nem található!");
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }
    }
}
