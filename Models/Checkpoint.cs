using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace worksystem.Models
{
    public enum SessionStatus
    {
        Active,
        Inactive
    }


    public class Checkpoint
    {
    [Key]
    public int CheckpointId { get; set; }

    [ForeignKey("Employee")]
    public int EmployeeId { get; set; }
    public required Employee Employee { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public required SessionStatus SessionStatus { get; set; }
    }
    
}