using worksystem.Services;

namespace worksystem.Services
{
    public static class ServiceExtensions
    {
         public static void RegisterAppServices(this IServiceCollection services)
        {
            services.AddScoped<ICheckpointService, CheckpointService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IMonthlyreportService, MonthlyreportService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<IWorklogService, WorklogService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
        }
    }
}
