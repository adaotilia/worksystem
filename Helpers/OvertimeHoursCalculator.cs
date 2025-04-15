using System;
using System.Collections.Generic;
using worksystem.Models;
using worksystem.DTOs;

namespace worksystem.Helpers
{
    public static class OvertimeHoursCalculator
    {
        //Túlóra számítása Schedule és Worklog alapján
         public static decimal CalculateOvertimeHours(Schedule schedule, decimal workHours)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            if (schedule.ScheduledHours <= 0 || workHours <= 0)
                return 0;

            return workHours - schedule.ScheduledHours;
        }
        //Túlóra számítása Schedule és Worklog alapján
        public static Dictionary<DateOnly, decimal> CalculateOvertimeHoursPerDay(
            IEnumerable<Schedule> schedules,
            Dictionary<DateOnly, decimal> workHoursPerDay)
        {
            if (schedules == null)
                throw new ArgumentNullException(nameof(schedules));
            if (workHoursPerDay == null)
                throw new ArgumentNullException(nameof(workHoursPerDay));

            return schedules
                .ToDictionary(
                    s => s.ScheduledDate,
                    s => workHoursPerDay.TryGetValue(s.ScheduledDate, out decimal workHours) 
                        ? CalculateOvertimeHours(s, workHours)
                        : -s.ScheduledHours
                );
        }
        //Havi túlóra számítása Monthlyreport és Schedule alapján napokra
        public static decimal CalculateMonthlyOvertimeHours(
            IEnumerable<Schedule> schedules,
            Dictionary<DateOnly, decimal> workHoursPerDay,
            DateOnly reportMonth)
        {
            if (schedules == null)
                throw new ArgumentNullException(nameof(schedules));
            if (workHoursPerDay == null)
                throw new ArgumentNullException(nameof(workHoursPerDay));

            decimal totalOvertime = 0;
            var dailyOvertimes = CalculateOvertimeHoursPerDay(schedules, workHoursPerDay);

            foreach (var overtime in dailyOvertimes)
            {
                if (overtime.Key.Month == reportMonth.Month && 
                    overtime.Key.Year == reportMonth.Year)
                {
                    totalOvertime += overtime.Value;
                }
            }
            return totalOvertime;
        }

        //Havi túlóra számítása Monthlyreport és Schedule alapján összes
        public static decimal CalculateMonthlyOvertimeHours(
            decimal monthlyWorkHours, 
            decimal monthlyScheduledHours 
        )
        {
            if (monthlyWorkHours > monthlyScheduledHours)
            {
                return monthlyWorkHours - monthlyScheduledHours;
            }
            return 0;
        }
    }
}