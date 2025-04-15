using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace worksystem.Models
{
    public enum ScheduleType
    {
        Shift,
        Overtime,
        DayOff,
        PaidTimeOff,
        SickLeave,
    }

    public class Schedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }
        public required Employee Employee { get; set; }
        
        public DateOnly ScheduledDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public ScheduleType Type { get; set; }

        public decimal ScheduledHours { get; set; }

        public decimal ScheduledMonthlyHours { get; set; }

        public decimal ScheduledWorkDays { get; set; }
    }
}