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
    [Authorize(Policy = "ManagerOrAbove")]
    [ApiController]
    [Route("[controller]")]
    public class ManagerController : ControllerBase
    { 
        private readonly ICheckpointService _checkpointService;
        private readonly IEmployeeService _employeeService;
        private readonly IMonthlyreportService _monthlyreportService;
        private readonly IScheduleService _scheduleService;
        private readonly IWorklogService _worklogService;

        public ManagerController(
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
                throw new UnauthorizedAccessException("Invalid Employee ID in token");
            return result;
        }
        // Checkpoint endpoints
        [HttpGet("checkpoints/{year}/{month}")]
        public async Task<IActionResult> GetCheckpointsByMonth(int year, int month)
        {
            var checkpoints = await _checkpointService.GetAllCheckpointsByMonth(year, month);
            return Ok(checkpoints);
        }

        [HttpGet("checkpoints/employee/{employeeId}/{year}/{month}")]
        public async Task<IActionResult> GetCheckpointsByEmployeeId(int employeeId, int year, int month)
        {
            var checkpoints = await _checkpointService.GetCheckpointsByEmployeeId(employeeId, year, month);
            return Ok(checkpoints);
        }

        [HttpGet("checkpoints/employee/{employeeId}/status/{date:datetime}")]
        public async Task<IActionResult> GetSessionStatusByEmployeeId(int employeeId, DateTime date)
        {
            var status = await _checkpointService.GetSessionStatusByEmployeeId(employeeId, date);
            return Ok(status);
        }

        // Employee endpoints
        [HttpGet("employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesByMonth(DateOnly.FromDateTime(DateTime.Now));
            return Ok(employees);
        }

        [HttpGet("employees/{employeeId}")]
        public async Task<IActionResult> GetEmployeeById(int employeeId)
        {
            var employee = await _employeeService.GetEmployeeById(employeeId);
            if (employee == null)
                return NotFound();
            return Ok(employee);
        }

        [HttpGet("employees/role/{role}")]
        public async Task<IActionResult> GetEmployeesByRole(UserRole role)
        {
            var employees = await _employeeService.GetEmployeesByRole(role);
            return Ok(employees);
        }

        [HttpGet("employees/username/{username}")]
        public async Task<IActionResult> GetEmployeeByUsername(string username)
        {
            var employee = await _employeeService.GetEmployeeByUsername(username);
            if (employee == null)
                return NotFound();
            return Ok(employee);
        }
        [HttpPut("employees/password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordDTO passwordDto)
        {
            try
            {
                return await _employeeService.UpdatePassword(passwordDto);
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
        // Monthlyreport endpoints
        [HttpGet("monthlyreports/{year}/{month}")]
        public async Task<IActionResult> GetMonthlyReports(int year, int month)
        {
            var reports = await _monthlyreportService.GetAllMonthlyreportsByReportMonth(new DateOnly(year, month, 1));
            return Ok(reports);
        }

        [HttpGet("monthlyreports/employee/{employeeId}")]
        public async Task<IActionResult> GetMonthlyReportsByEmployeeId(int employeeId)
        {
            var reports = await _monthlyreportService.GetMonthlyreportsByEmployeeId(employeeId);
            return Ok(reports);
        }

        // Schedule endpoints
        [HttpGet("schedules/{year}/{month}")]
        public async Task<IActionResult> GetSchedulesByMonth(int year, int month)
        {
            var date = new DateOnly(year, month, 1);
            var schedules = await _scheduleService.GetAllSchedulesByMonth(date);
            return Ok(schedules);
        }
        [HttpGet("schedules/employee/{employeeId}/{year}/{month}")]
        public async Task<IActionResult> GetSchedulesByEmployeeId(int employeeId, int year, int month)
        {
            var schedules = await _scheduleService.GetSchedulesByEmployeeId(employeeId);
            return Ok(schedules);
        }

        [HttpGet("schedules/date/{year}/{month}/{day}")]
        public async Task<IActionResult> GetSchedulesByDate(int year, int month, int day)
        {
            var date = new DateOnly(year, month, day);
            var schedules = await _scheduleService.GetSchedulesByDate(date);
            return Ok(schedules);
        }

        [HttpPost("schedules")]
        public async Task<IActionResult> CreateSchedule([FromQuery] int employeeId, [FromBody] ScheduleDTO schedule)
        {
            try
            {
                var createdSchedule = await _scheduleService.CreateSchedule(employeeId, schedule);
                return CreatedAtAction(nameof(GetSchedulesByEmployeeId), new { employeeId = createdSchedule.EmployeeId, year = DateTime.Now.Year, month = DateTime.Now.Month }, createdSchedule);
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

        [HttpPut("schedules/employee/{employeeId}/{year}/{month}/{day}")]
        public async Task<IActionResult> UpdateSchedule(int employeeId, int year, int month, int day, [FromBody] ScheduleDTO schedule)
        {
            try
            {
                var updatedSchedule = await _scheduleService.UpdateSchedule(employeeId, year, month, day, schedule);
                return Ok(updatedSchedule);
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

        [HttpDelete("schedules/employee/{employeeId}/{year}/{month}/{day}")]
        public async Task<IActionResult> DeleteSchedule(int employeeId, int year, int month, int day)
        {
            try
            {
                await _scheduleService.DeleteSchedule(employeeId, year, month, day);
                return NoContent();
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

        // Worklog endpoints
        [HttpGet("worklogs/{year}/{month}")]
        public async Task<IActionResult> GetWorklogsByMonth(int year, int month)
        {
            var date = new DateOnly(year, month, 1);
            var worklogs = await _worklogService.GetAllWorklogsByMonth(date);
            return Ok(worklogs);
        }

        [HttpGet("worklogs/employee/{employeeId}/{year}/{month}")]
        public async Task<IActionResult> GetWorklogsByEmployeeId(int employeeId, int year, int month)
        {
            var date = new DateOnly(year, month, 1);
            var worklogs = await _worklogService.GetWorklogsByEmployeeId(employeeId, date);
            return Ok(worklogs);
        }

        [HttpGet("worklogs/date/{year}/{month}/{day}")]
        public async Task<IActionResult> GetWorklogsByDate(int year, int month, int day)
        {
            var date = new DateOnly(year, month, day);
            var worklogs = await _worklogService.GetWorklogsByDate(date);
            return Ok(worklogs);
        }
    }
}