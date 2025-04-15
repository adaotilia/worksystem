using System.Collections.Generic;
using System.Threading.Tasks;
using worksystem.DTOs;
using worksystem.Models;
using worksystem.Data;
using Microsoft.EntityFrameworkCore;

namespace worksystem.Services
{
    public class CheckpointService : ICheckpointService
    {
        private readonly AppDbContext _context;

        public CheckpointService(AppDbContext context)
        {
            _context = context;
        }

        //Összes Checkpoint lekérése egy megadott hónapra vonatkozóan.
        public async Task<List<CheckpointDTO>> GetAllCheckpointsByMonth(int year, int month)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var checkpoints = await _context.Checkpoints
                .Include(c => c.Employee)
                .Where(c => c.CheckInTime.HasValue && 
                    c.CheckInTime.Value >= firstDayOfMonth && 
                    c.CheckInTime.Value <= lastDayOfMonth)
                .ToListAsync();

            var result = checkpoints.Select(c =>
            {
                var status = CalculateStatus(c);

                return new CheckpointDTO
                {
                    CheckpointId = c.CheckpointId,
                    EmployeeId = c.EmployeeId,
                    CheckInTime = c.CheckInTime,
                    CheckOutTime = c.CheckOutTime,
                    SessionStatus = status
                };
            }).ToList();

            return result;
        }
        //A megadott ID-hoz tartozó kiválasztott havi Checkpoint adat.
        public async Task<List<CheckpointDTO>> GetCheckpointsByEmployeeId(int EmployeeId, int year, int month)
        {
             var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var checkpoints = await _context.Checkpoints
                .Where(c => c.EmployeeId == EmployeeId)
                .Where(c => c.CheckInTime >= firstDayOfMonth && c.CheckInTime <= lastDayOfMonth)
                .Include(c => c.Employee)
                .ToListAsync();

            return checkpoints.Select(c => new CheckpointDTO
            {
                CheckpointId = c.CheckpointId,
                EmployeeId = c.EmployeeId,
                CheckInTime = c.CheckInTime,
                CheckOutTime = c.CheckOutTime,
                SessionStatus = CalculateStatus(c)
            }).ToList();
        }
        //A megadott ID-hoz tartozó az adott időpontban érvényes Status.(Active/Inactive)
        public async Task<SessionStatus> GetSessionStatusByEmployeeId(int EmployeeId, DateTime date)
        {
            var checkpoint = await _context.Checkpoints
                .Where(c => c.EmployeeId == EmployeeId)
                .OrderByDescending(c => c.CheckInTime)
                .FirstOrDefaultAsync();

            if (checkpoint == null)
                return SessionStatus.Inactive;

            return CalculateStatus(checkpoint);
        }
        //A megadott ID-hoz tartozó összes CheckinTime és CheckoutTime egy hónapban.
        public async Task<(List<DateTime?> CheckinTimes, List<DateTime> CheckoutTimes)> GetCheckTimesByEmployeeId(int EmployeeId, int year, int month)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var checkpoints = await _context.Checkpoints
                .Where(c => c.EmployeeId == EmployeeId)
                .Where(c => c.CheckInTime >= firstDayOfMonth && c.CheckInTime <= lastDayOfMonth)
                .ToListAsync();

            var checkinTimes = checkpoints
                .Where(c => c.CheckInTime != null)
                .OrderByDescending(c => c.CheckInTime)
                .Select(c => c.CheckInTime)
                .ToList();

            var checkoutTimes = checkpoints
                .Where(c => c.CheckOutTime != null)
                .OrderByDescending(c => c.CheckOutTime)
                .Select(c => c.CheckOutTime.Value)
                .ToList();

            return (checkinTimes, checkoutTimes);
        }
        // CheckinTime és CheckoutTime manuális hozzáadására alkalmas.
        public async Task<CheckpointDTO> CreateCheckpoint(CheckpointDTO checkpoint)
        {
            if (checkpoint.CheckInTime == null)
            {
                throw new ArgumentException("A Belépési időpont kötelező a checkpoint létrehozásához");
            }

            var employee = await _context.Employees
                .Where(e => e.EmployeeId == checkpoint.EmployeeId)
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                throw new InvalidOperationException("A megadott dolgozó nem létezik.");
            }

            var checkpointEntity = new Checkpoint
            {
                EmployeeId = checkpoint.EmployeeId,
                Employee = employee,
                CheckInTime = checkpoint.CheckInTime,
                CheckOutTime = checkpoint.CheckOutTime,
                SessionStatus = CalculateStatus(checkpoint)
            };

            _context.Checkpoints.Add(checkpointEntity);
            await _context.SaveChangesAsync();

            var checkin = checkpoint.CheckInTime ?? DateTime.Now;
            var reportMonth = new DateOnly(checkin.Year, checkin.Month, 1);

            var allCheckpoints = await _context.Checkpoints
                .Where(c => c.EmployeeId == checkpoint.EmployeeId &&
                            c.CheckInTime.HasValue &&
                            c.CheckInTime.Value.Year == reportMonth.Year &&
                            c.CheckInTime.Value.Month == reportMonth.Month)
                .ToListAsync();

            var checkpointDtos = allCheckpoints.Select(c => new worksystem.DTOs.CheckpointDTO
            {
                CheckpointId = c.CheckpointId,
                EmployeeId = c.EmployeeId,
                CheckInTime = c.CheckInTime,
                CheckOutTime = c.CheckOutTime,
                SessionStatus = c.SessionStatus
            }).ToList();

            var monthlyReport = await _context.Monthlyreports
                .FirstOrDefaultAsync(mr => mr.EmployeeId == checkpoint.EmployeeId && mr.ReportMonth == reportMonth);

            if (monthlyReport == null)
            {
                monthlyReport = new worksystem.Models.Monthlyreport
                {
                    EmployeeId = checkpoint.EmployeeId,
                    Employee = employee,
                    ReportMonth = reportMonth,
                };
                _context.Monthlyreports.Add(monthlyReport);
            }

            var monthlyReportService = new MonthlyreportService(_context);
            var updatedMonthlyReport = await monthlyReportService.UpdateOrCreateMonthlyReportsPerDay(checkpoint.EmployeeId, reportMonth.Year, reportMonth.Month);

            foreach (var monthlyReportUpdate in updatedMonthlyReport)
            {
                monthlyReport.WorkHours = monthlyReportUpdate.WorkHours;
                monthlyReport.OvertimeHours = monthlyReportUpdate.OvertimeHours;
                monthlyReport.MonthlyWorkDays = monthlyReportUpdate.MonthlyWorkDays;
                monthlyReport.MonthlyWorkHours = monthlyReportUpdate.MonthlyWorkHours;
                monthlyReport.MonthlyOvertimeHours = monthlyReportUpdate.MonthlyOvertimeHours;
            }

            await _context.SaveChangesAsync();

            return new CheckpointDTO
            {
                CheckpointId = checkpointEntity.CheckpointId,
                EmployeeId = checkpointEntity.EmployeeId,
                CheckInTime = checkpointEntity.CheckInTime,
                CheckOutTime = checkpointEntity.CheckOutTime,
                SessionStatus = checkpointEntity.SessionStatus
            };
        }
        //CheckinTime vagy CheckoutTime módosítására alkalmas.
        public async Task<CheckpointDTO> UpdateCheckpoint(int EmployeeId, CheckpointDTO checkpoint)
        {
            var activeCheckpoint = await _context.Checkpoints
                .Where(c => c.EmployeeId == EmployeeId)
                .Where(c => c.CheckInTime != null)
                .Where(c => c.CheckOutTime == null)
                .FirstOrDefaultAsync();

            if (activeCheckpoint == null)
            {
                throw new InvalidOperationException("Nincs aktív Belépési idő a munkavállalóhoz, a Hozzáadás lehetőséget használja.");
            }

            if (checkpoint.CheckInTime != null)
            {
                activeCheckpoint.CheckInTime = checkpoint.CheckInTime;
            }

            if (checkpoint.CheckOutTime != null)
            {
                activeCheckpoint.CheckOutTime = checkpoint.CheckOutTime;
                activeCheckpoint.SessionStatus = SessionStatus.Inactive;
            }
            else
            {
                activeCheckpoint.SessionStatus = CalculateStatus(activeCheckpoint);
            }

            await _context.SaveChangesAsync();

            return new CheckpointDTO
            {
                CheckpointId = activeCheckpoint.CheckpointId,
                EmployeeId = activeCheckpoint.EmployeeId,
                CheckInTime = activeCheckpoint.CheckInTime,
                CheckOutTime = activeCheckpoint.CheckOutTime,
                SessionStatus = activeCheckpoint.SessionStatus
            };
        }
        //CheckinTime és CheckoutTime törlésére alkalmas.
        public async Task DeleteCheckpoint(int EmployeeId)
        {
            var activeCheckpoint = await _context.Checkpoints
                .Where(c => c.EmployeeId == EmployeeId)
                .Where(c => c.CheckInTime != null)
                .Where(c => c.CheckOutTime == null)
                .FirstOrDefaultAsync();

            if (activeCheckpoint == null)
            {
                throw new InvalidOperationException("Nincs aktív Belépési idő a munkavállalóhoz.");
            }

            activeCheckpoint.CheckInTime = null;
            activeCheckpoint.CheckOutTime = null;
            activeCheckpoint.SessionStatus = SessionStatus.Inactive;

            await _context.SaveChangesAsync();
        }
        //Privát metódus a Status megadásához: Active/Inactive. Model-hez
        protected SessionStatus CalculateStatus(Checkpoint checkpoint)
        {
            if (checkpoint == null)
                return SessionStatus.Inactive;

            if (checkpoint.CheckInTime == null)
                return SessionStatus.Inactive;

            if (checkpoint.CheckOutTime != null)
                return SessionStatus.Inactive;

            return SessionStatus.Active;
        }
        //Privát metódus a Status megadásához: Active/Inactive., DTO-hoz
        protected SessionStatus CalculateStatus(CheckpointDTO checkpoint)
        {
            if (checkpoint == null)
                return SessionStatus.Inactive;

            if (checkpoint.CheckInTime == null)
                return SessionStatus.Inactive;

            if (checkpoint.CheckOutTime != null)
                return SessionStatus.Inactive;

            return SessionStatus.Active;
        }
    }
}
