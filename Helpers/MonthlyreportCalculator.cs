using System;
using System.Collections.Generic;
using worksystem.Models;
using worksystem.DTOs;

namespace worksystem.Helpers
{
    public static class MonthlyreportCalculator
    {
        // Egy checkpoint ledolgozott óráinak kiszámítása (ha van mindkét időpont)
        public static decimal CalculateWorkHoursForCheckpoint(CheckpointDTO checkpoint)
        {
            if (checkpoint.CheckInTime.HasValue && checkpoint.CheckOutTime.HasValue)
            {
                var diff = checkpoint.CheckOutTime.Value - checkpoint.CheckInTime.Value;
                return (decimal)diff.TotalHours > 0 ? (decimal)diff.TotalHours : 0;
            }
            return 0;
        }

        // Napi/havi ledolgozott órák checkpointokból
        public static decimal CalculateMonthlyWorkHoursFromCheckpoints(IEnumerable<CheckpointDTO> checkpoints, int year, int month)
        {
            if (checkpoints == null)
                throw new ArgumentNullException(nameof(checkpoints));

            decimal totalHours = 0;
            foreach (var cp in checkpoints)
            {
                if (cp.CheckInTime.HasValue && cp.CheckOutTime.HasValue &&
                    cp.CheckInTime.Value.Year == year && cp.CheckInTime.Value.Month == month)
                {
                    totalHours += CalculateWorkHoursForCheckpoint(cp);
                }
            }
            return totalHours;
        }

        // Havi ledolgozott napok száma checkpointokból
        public static decimal CalculateMonthlyWorkDaysFromCheckpoints(IEnumerable<CheckpointDTO> checkpoints, int year, int month)
        {
            if (checkpoints == null)
                throw new ArgumentNullException(nameof(checkpoints));

            var workDays = new HashSet<DateTime>();
            foreach (var cp in checkpoints)
            {
                if (cp.CheckInTime.HasValue && cp.CheckOutTime.HasValue &&
                    cp.CheckInTime.Value.Year == year && cp.CheckInTime.Value.Month == month)
                {
                    workDays.Add(cp.CheckInTime.Value.Date);
                }
            }
            return workDays.Count;
        }

        // Havi ledolgozott órák Monthlyreportból
        public static decimal CalculateMonthlyWorkHours(IEnumerable<Monthlyreport> reports, DateOnly reportMonth)
        {
            if (reports == null)
                throw new ArgumentNullException(nameof(reports));

            decimal totalHours = 0;
            foreach (var report in reports)
            {
                if (report.ReportMonth.Month == reportMonth.Month &&
                    report.ReportMonth.Year == reportMonth.Year)
                {
                    totalHours += report.WorkHours;
                }
            }
            return totalHours;
        }
    }
}
