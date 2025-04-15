using System;
using worksystem.Models;

namespace worksystem.DTOs
{
    public class ScheduleDTO
    {
        public int ScheduleId { get; set; }
        public int EmployeeId { get; set; }
        public required string? FullName { get; set; }
        public DateOnly ScheduledDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public ScheduleType Type { get; set; }
        public decimal ScheduledHours { get; set; }
        public decimal ScheduledMonthlyHours { get; set; }
        public decimal ScheduledWorkDays { get; set; }
    }
}
