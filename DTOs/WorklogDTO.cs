using System;
using System.ComponentModel.DataAnnotations;
using worksystem.Models;

namespace worksystem.DTOs
{
    public class WorklogDTO
    {
    public int WorklogId { get; set; }
    public required int EmployeeId { get; set; }
    public required string FullName { get; set; }
    public required DateOnly ScheduledDate { get; set; }
    public required decimal WorkHours { get; set; }
    public required decimal OvertimeHours { get; set; }
    public decimal ScheduledHours { get; set; }
    public decimal ScheduledOvertime { get; set; }
    public ScheduleType Type { get; set; }
    public decimal MonthlyWorkDays { get; set; }
    public decimal ScheduledWorkDays { get; set; }
    public int MonthlyOvertimeHours { get; set; }
    }
}