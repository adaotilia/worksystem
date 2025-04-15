using AutoMapper;
using worksystem.DTOs;
using worksystem.Models;

namespace worksystem.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
        // Checkpoint <-> CheckpointDTO közötti kétirányú leképzés
            CreateMap<Checkpoint, CheckpointDTO>()
                .ForMember(dest => dest.CheckpointId, opt => opt.MapFrom(src => src.CheckpointId))
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.CheckInTime, opt => opt.MapFrom(src => src.CheckInTime))
                .ForMember(dest => dest.CheckOutTime, opt => opt.MapFrom(src => src.CheckOutTime))
                .ForMember(dest => dest.SessionStatus, opt => opt.MapFrom(src => src.SessionStatus));

            CreateMap<CheckpointDTO, Checkpoint>()
                .ForMember(dest => dest.CheckpointId, opt => opt.MapFrom(src => src.CheckpointId))
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.CheckInTime, opt => opt.MapFrom(src => src.CheckInTime))
                .ForMember(dest => dest.CheckOutTime, opt => opt.MapFrom(src => src.CheckOutTime))
                .ForMember(dest => dest.SessionStatus, opt => opt.MapFrom(src => src.SessionStatus));
        

         // Employee <-> EmployeeDTO közötti kétirányú leképzés
            CreateMap<Employee, EmployeeDTO>()
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.PasswordHash))
                .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.UserRole));

            CreateMap<EmployeeDTO, Employee>()
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.UserRole));

            // Monthlyreport <-> MonthlyreportDTO közötti kétirányú leképzés
            CreateMap<Monthlyreport, MonthlyreportDTO>()
                .ForMember(dest => dest.MonthlyreportId, opt => opt.MapFrom(src => src.MonthlyreportId))
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.ReportMonth, opt => opt.MapFrom(src => src.ReportMonth))
                .ForMember(dest => dest.CheckInTime, opt => opt.Ignore())
                .ForMember(dest => dest.CheckOutTime, opt => opt.Ignore())
                .ForMember(dest => dest.WorkHours, opt => opt.MapFrom(src => src.WorkHours))
                .ForMember(dest => dest.OvertimeHours, opt => opt.MapFrom(src => src.OvertimeHours))
                .ForMember(dest => dest.MonthlyWorkDays, opt => opt.MapFrom(src => src.MonthlyWorkDays))
                .ForMember(dest => dest.MonthlyWorkHours, opt => opt.MapFrom(src => src.MonthlyWorkHours))
                .ForMember(dest => dest.MonthlyOvertimeHours, opt => opt.MapFrom(src => src.MonthlyOvertimeHours));

            CreateMap<MonthlyreportDTO, Monthlyreport>()
                .ForMember(dest => dest.MonthlyreportId, opt => opt.MapFrom(src => src.MonthlyreportId))
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.ReportMonth, opt => opt.MapFrom(src => src.ReportMonth))
                .ForMember(dest => dest.WorkHours, opt => opt.MapFrom(src => src.WorkHours))
                .ForMember(dest => dest.OvertimeHours, opt => opt.MapFrom(src => src.OvertimeHours))
                .ForMember(dest => dest.MonthlyWorkDays, opt => opt.MapFrom(src => src.MonthlyWorkDays))
                .ForMember(dest => dest.MonthlyWorkHours, opt => opt.MapFrom(src => src.MonthlyWorkHours))
                .ForMember(dest => dest.MonthlyOvertimeHours, opt => opt.MapFrom(src => src.MonthlyOvertimeHours));
        
       // Schedule <-> ScheduleDTO közötti kétirányú leképzés
            CreateMap<Schedule, ScheduleDTO>()
                .ForMember(dest => dest.ScheduleId, opt => opt.MapFrom(src => src.ScheduleId))
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.ScheduledDate, opt => opt.MapFrom(src => src.ScheduledDate))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ScheduledHours, opt => opt.MapFrom(src => src.ScheduledHours))
                .ForMember(dest => dest.ScheduledWorkDays, opt => opt.MapFrom(src => src.ScheduledWorkDays));

            CreateMap<ScheduleDTO, Schedule>()
                .ForMember(dest => dest.ScheduleId, opt => opt.MapFrom(src => src.ScheduleId))
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.ScheduledDate, opt => opt.MapFrom(src => src.ScheduledDate))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ScheduledHours, opt => opt.MapFrom(src => src.ScheduledHours))
                .ForMember(dest => dest.ScheduledWorkDays, opt => opt.MapFrom(src => src.ScheduledWorkDays));

        // Worklog to WorklogDTO
            CreateMap<Worklog, WorklogDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.ScheduledWorkDays, opt => opt.MapFrom(src => src.ScheduledWorkDays))
                .ForMember(dest => dest.MonthlyWorkDays, opt => opt.MapFrom(src => src.MonthlyWorkDays));

            // WorklogDTO to Worklog
            CreateMap<WorklogDTO, Worklog>()
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.ScheduledDate, opt => opt.MapFrom(src => src.ScheduledDate))
                .ForMember(dest => dest.WorkHours, opt => opt.MapFrom(src => src.WorkHours))
                .ForMember(dest => dest.OvertimeHours, opt => opt.MapFrom(src => src.OvertimeHours))
                .ForMember(dest => dest.ScheduledHours, opt => opt.MapFrom(src => src.ScheduledHours))
                .ForMember(dest => dest.ScheduledOvertime, opt => opt.MapFrom(src => src.ScheduledOvertime))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));
            // RegisterRequest to EmployeeDTO
            CreateMap<RegisterRequest, EmployeeDTO>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                    .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                    .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                    .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.UserRole));        
        }
    }
}