using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace worksystem.Models
{
    public class Worklog
    {
        public int WorklogId { get; set; }
        public required int EmployeeId { get; set; }
        public required DateOnly ScheduledDate { get; set; }
        public required Employee Employee { get; set; }
        public int? WorkHours { get; set; }
        public int? OvertimeHours { get; set; }
        public int? ScheduledHours { get; set; }
        public int? ScheduledOvertime { get; set; }
        public int Type { get; set; }
        public decimal? MonthlyWorkDays { get; set; }
        public int? ScheduledWorkDays { get; set; }
        public int? MonthlyOvertimeHours { get; set; }
    }
}
