using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace worksystem.Models
{
    public class Monthlyreport
    {
        [Key]
        public int MonthlyreportId { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }
        public required Employee Employee { get; set; }
        
        public DateOnly ReportMonth { get; set; }

        public DateOnly Date { get; set; }

        public decimal WorkHours { get; set; }
        
        public decimal OvertimeHours { get; set; }
        
        public decimal MonthlyWorkDays { get; set; }
        
        public decimal MonthlyWorkHours { get; set; }
        
        public decimal MonthlyOvertimeHours { get; set; }
    }
}