using System;
using worksystem.Models;

namespace worksystem.DTOs
{
    public class MonthlyreportDTO
    {
        public int MonthlyreportId { get; set; }
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public DateOnly ReportMonth { get; set; }
        public DateOnly Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal WorkHours { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal MonthlyWorkDays { get; set; }
        public decimal MonthlyWorkHours { get; set; }
        public decimal MonthlyOvertimeHours { get; set; }
    }
}