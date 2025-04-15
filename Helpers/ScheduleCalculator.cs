using System;
using System.Collections.Generic;
using worksystem.Models;
using worksystem.DTOs;

namespace worksystem.Helpers
{
    public static class ScheduleCalculator
    {
        // Egy beosztás (Schedule) óráinak kiszámítása
        public static decimal CalculateHours(TimeOnly StartTime, TimeOnly EndTime)
        {
            var diff = EndTime - StartTime;
            if (diff.TotalHours < 0)
            {
                diff = diff.Add(TimeSpan.FromHours(24));
            }
            return (decimal)diff.TotalHours;
        }

        // Hány beosztott műszak/túlóra van (ahol mindkét időpont megvan)
        public static int CountScheduledShiftsAndOvertimes(List<Schedule> schedules)
        {
            int count = 0;
            foreach (var schedule in schedules)
            {
                if ((schedule.Type == ScheduleType.Shift || schedule.Type == ScheduleType.Overtime)
                    && schedule.StartTime != default && schedule.EndTime != default)
                {
                    count++;
                }
            }
            return count;
        }

        // Havi beosztott órák összeszámolása (adott év-hónap)
        public static decimal CalculateMonthlyScheduledHours(IEnumerable<Schedule> schedules, int year, int month)
        {
            if (schedules == null)
                throw new ArgumentNullException(nameof(schedules));
            decimal total = 0;
            foreach (var s in schedules)
            {
                if (s.ScheduledDate.Year == year && s.ScheduledDate.Month == month &&
                    s.StartTime != default && s.EndTime != default)
                {
                    total += CalculateHours(s.StartTime, s.EndTime);
                }
            }
            return total;
        }
    }
}
