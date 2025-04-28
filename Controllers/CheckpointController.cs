using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using worksystem.Services;
using worksystem.Models;
using worksystem.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace worksystem.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class CheckpointController : ControllerBase
    {
        private readonly ICheckpointService _checkpointService;
        private readonly IEmployeeService _employeeService;

        public CheckpointController(
            ICheckpointService checkpointService,
            IEmployeeService employeeService)
        {
            _checkpointService = checkpointService;
            _employeeService = employeeService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartWork(string employeeId)
        {
            if (!int.TryParse(employeeId, out var employeeIdInt))
            {
                return BadRequest("Hibás dolgozó azonosító formátum.");
            }

            try
            {
                var activeCheckpoint = await _checkpointService.GetSessionStatusByEmployeeId(employeeIdInt, DateTime.Now);
                
                if (activeCheckpoint == SessionStatus.Active)
                {
                    return BadRequest("Már van aktív munkafolyamat.");
                }

                var employee = await _employeeService.GetEmployeeById(employeeIdInt);
                var newCheckpoint = new CheckpointDTO
                {
                    EmployeeId = employeeIdInt,
                    CheckInTime = DateTime.Now,
                    CheckOutTime = null,
                    SessionStatus = SessionStatus.Active
                };

                await _checkpointService.CreateCheckpoint(newCheckpoint);

                return Ok(new 
                {
                    employeeName = employee.Username,
                    confirmation = "A megadott azonosítóhoz tartozó felhasználónév a következő: " + employee.Username
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Nincs ilyen dolgozó azonosító.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Belső szerverhiba: " + ex.Message);
            }
        }

        [HttpPost("end")]
        public async Task<IActionResult> EndWork(string employeeId)
        {
            if (!int.TryParse(employeeId, out var employeeIdInt))
            {
                return BadRequest("Hibás dolgozó azonosító formátum.");
            }

            var activeCheckpoint = await _checkpointService.GetSessionStatusByEmployeeId(employeeIdInt, DateTime.Now);
            
            if (activeCheckpoint != SessionStatus.Active)
            {
                return BadRequest("Nincs aktív munkafolyamat.");
            }

            var employee = await _employeeService.GetEmployeeById(employeeIdInt);
            if (employee == null)
            {
                return BadRequest("Nem létező dolgozó azonosító.");
            }

            var checkpoint = await _checkpointService.GetCheckpointsByEmployeeId(employeeIdInt, DateTime.Now.Year, DateTime.Now.Month);
            var lastCheckpoint = checkpoint.OrderByDescending(c => c.CheckInTime).FirstOrDefault();

            if (lastCheckpoint == null || lastCheckpoint.CheckOutTime != null || lastCheckpoint.CheckInTime == null)
            {
                return BadRequest("Nincs aktív munkafolyamat.");
            }

            var updateDto = new CheckpointDTO
            {
                CheckInTime = lastCheckpoint.CheckInTime,
                CheckOutTime = DateTime.Now,
                SessionStatus = SessionStatus.Inactive
            };

            await _checkpointService.UpdateCheckpoint(employeeIdInt, lastCheckpoint.CheckpointId, updateDto);

            return Ok(new 
            { 
                employeeName = employee.Username,
                duration = updateDto.CheckOutTime.Value - updateDto.CheckInTime,
                confirmation = "A megadott azonosítóhoz tartozó felhasználónév a következő: " + employee.Username
            });
        }
    }
}