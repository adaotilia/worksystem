using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using worksystem.Services;
using worksystem.DTOs;
using worksystem.Models;
using System.Security.Claims;

namespace worksystem.Controllers
{
    [Authorize(Policy = "EmployeeOrAbove")]
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    { 
        private readonly ICheckpointService _checkpointService;
        private readonly IEmployeeService _employeeService;
        private readonly IMonthlyreportService _monthlyreportService;
        private readonly IScheduleService _scheduleService;
        private readonly IWorklogService _worklogService;

        public EmployeeController(
            ICheckpointService checkpointService,
            IEmployeeService employeeService,
            IMonthlyreportService monthlyreportService,
            IScheduleService scheduleService,
            IWorklogService worklogService)
        {
            _checkpointService = checkpointService;
            _employeeService = employeeService;
            _monthlyreportService = monthlyreportService;
            _scheduleService = scheduleService;
            _worklogService = worklogService;
        }
        private int GetEmployeeIdFromToken()
        {
            var employeeId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(employeeId) || !int.TryParse(employeeId, out int result))
                throw new UnauthorizedAccessException("Érvénytelen dolgozó azonosító");
            return result;
        }
        //Checkpoint endpoints
        [HttpGet("checkpoints/{year}/{month}")]
        public async Task<IActionResult> GetCheckpointsByEmployeeId(int year, int month)
        {
            var employeeId = GetEmployeeIdFromToken();
            var checkpoints = await _checkpointService.GetCheckpointsByEmployeeId(employeeId, year, month);
            return Ok(checkpoints);
        }
        [HttpGet("checkpoints/status/{date:datetime}")]
        public async Task<IActionResult> GetSessionStatus(int year, int month, DateTime date)
        {
            var employeeId = GetEmployeeIdFromToken(); 
            var status = await _checkpointService.GetSessionStatusByEmployeeId(employeeId, date);
            return Ok(status);
        }
        [HttpGet("checkpoints/times/{year}/{month}")]
        public async Task<IActionResult> GetCheckTimes(int year, int month)
        {
            var employeeId = GetEmployeeIdFromToken();
            var times = await _checkpointService.GetCheckTimesByEmployeeId(employeeId, year, month);
            return Ok(times);
        }
        //Employee endpoints
        [HttpGet("employees/me")]
        public async Task<IActionResult> GetEmployeeMe()
        {
            var employeeId = GetEmployeeIdFromToken();
            var employee = await _employeeService.GetEmployeeById(employeeId);
            if (employee == null)
                return NotFound();
            return Ok(employee);
        }
        [HttpPut("employees/password")]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordDTO password)
        {
            var employeeId = GetEmployeeIdFromToken();
            return await _employeeService.UpdatePassword(password);
        }
        //Monthlyreport endpoints
        [HttpGet("monthlyreports/me")]
        public async Task<IActionResult> GetMonthlyReportsByEmployeeId()
        {
            var employeeId = GetEmployeeIdFromToken(); 
            var reports = await _monthlyreportService.GetMonthlyreportsByEmployeeId(employeeId);
            return Ok(reports);
        }
        //Schedule endpoints
        [HttpGet("schedules/{year}/{month}")]
        public async Task<IActionResult> GetSchedules(int year, int month)
        {
            var employeeId = GetEmployeeIdFromToken();
            var schedules = await _scheduleService.GetSchedulesByEmployeeId(employeeId);
            return Ok(schedules);
        }
        [HttpGet("schedules/date/{year}/{month}/{day}")]
        public async Task<IActionResult> GetSchedulesByDate(int year, int month, int day)
        {
            var employeeId = GetEmployeeIdFromToken(); 
            var date = new DateOnly(year, month, day);
            var schedules = await _scheduleService.GetSchedulesByEmployeeId(employeeId);
            return Ok(schedules);
        }
        //Worklog endpoints
        [HttpGet("worklogs/{year}/{month}")]
        public async Task<IActionResult> GetWorklogsByMonth(int year, int month)
        {
            var employeeId = GetEmployeeIdFromToken(); 
            var date = new DateOnly(year, month, 1);
            var worklogs = await _worklogService.GetWorklogsByEmployeeId(employeeId, date);
            return Ok(worklogs);
        }

        [HttpGet("worklogs/date/{year}/{month}/{day}")]
        public async Task<IActionResult> GetWorklogsByDate(int year, int month, int day)
        {
            var employeeId = GetEmployeeIdFromToken(); 
            var date = new DateOnly(year, month, day);
            var worklogs = await _worklogService.GetWorklogsByEmployeeId(employeeId, date);
            return Ok(worklogs);
        }
    }
}