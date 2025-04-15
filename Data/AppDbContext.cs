using Microsoft.EntityFrameworkCore;
using worksystem.Models;

namespace worksystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { 
        }
        public DbSet<Checkpoint> Checkpoints { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Monthlyreport> Monthlyreports { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Worklog> Worklogs { get; set; }
    }
}