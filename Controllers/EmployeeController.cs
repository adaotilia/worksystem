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
        [HttpGet("checkpoints/status/{year:int}/{month:int}/{day:int}")]
        public async Task<IActionResult> GetSessionStatus(int year, int month, int day)
        {
            var employeeId = GetEmployeeIdFromToken(); 
            var date = new DateTime(year, month, day);
            var status = await _checkpointService.GetSessionStatusByEmployeeId(employeeId, date);
            return Ok(status);
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
            try
            {
                var employeeId = GetEmployeeIdFromToken();
                return await _employeeService.UpdatePassword(password);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ismeretlen hiba történt." });
            }
        }
        //Monthlyreport endpoints
        [HttpGet("monthlyreports/me/bydate")]
        public async Task<IActionResult> GetMonthlyReportByEmployeeIdAndDate([FromQuery] int year, [FromQuery] int month)
        {
            var employeeId = GetEmployeeIdFromToken();
            var reports = await _monthlyreportService.GetMonthlyreportsByEmployeeId(employeeId);
            var filteredReports = reports
                .Where(r => r.ReportMonth.Year == year && r.ReportMonth.Month == month)
                .ToList();
            if (filteredReports == null || !filteredReports.Any())
                return NotFound($"Nincs jelentés erre az évre és hónapra: {year}-{month}");
            return Ok(filteredReports);
        }
        //Schedule endpoints
        [HttpGet("schedules/{year}/{month}")]
        public async Task<IActionResult> GetSchedules(int year, int month)
        {
            try
            {
                var employeeId = GetEmployeeIdFromToken();
                var schedules = await _scheduleService.GetSchedulesByEmployeeId(employeeId);
                return Ok(schedules);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ismeretlen hiba történt." });
            }
        }
        [HttpGet("schedules/date/{year}/{month}/{day}")]
        public async Task<IActionResult> GetSchedulesByDate(int year, int month, int day)
        {
            try
            {
                var employeeId = GetEmployeeIdFromToken(); 
                var date = new DateOnly(year, month, day);
                var schedules = await _scheduleService.GetSchedulesByEmployeeId(employeeId);
                return Ok(schedules);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ismeretlen hiba történt." });
            }
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