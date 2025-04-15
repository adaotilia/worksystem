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
            var activeCheckpoint = await _checkpointService.GetSessionStatusByEmployeeId(int.Parse(employeeId), DateTime.Now);
            
            if (activeCheckpoint == SessionStatus.Active)
            {
                return BadRequest("Már van aktív munkafolyamat.");
            }

            var employee = await _employeeService.GetEmployeeById(int.Parse(employeeId));
            if (employee == null)
            {
                return BadRequest("Nem létező dolgozó azonosító.");
            }

            return Ok(new 
            {
                message = "Munka elkezdődött.",
                employeeName = employee.Username,
                confirmation = "A megadott azonosítóhoz tartozó felhasználónév a következő: " + employee.Username + ". Megerősíti a munkaba való belépést?"
            });
        }

        [HttpPost("end")]
        public async Task<IActionResult> EndWork(string employeeId)
        {
            var activeCheckpoint = await _checkpointService.GetSessionStatusByEmployeeId(int.Parse(employeeId), DateTime.Now);
            
            if (activeCheckpoint != SessionStatus.Active)
            {
                return BadRequest("Nincs aktív munkafolyamat.");
            }

            var employee = await _employeeService.GetEmployeeById(int.Parse(employeeId));
            if (employee == null)
            {
                return BadRequest("Nem létező dolgozó azonosító.");
            }

            var checkpoint = await _checkpointService.GetCheckpointsByEmployeeId(int.Parse(employeeId), DateTime.Now.Year, DateTime.Now.Month);
            var lastCheckpoint = checkpoint.OrderByDescending(c => c.CheckInTime).FirstOrDefault();

            if (lastCheckpoint == null || lastCheckpoint.CheckOutTime != null)
            {
                return BadRequest("Nincs aktív munkafolyamat.");
            }

           lastCheckpoint.CheckOutTime = DateTime.Now;
            lastCheckpoint.SessionStatus = SessionStatus.Inactive;

            await _checkpointService.UpdateCheckpoint(int.Parse(employeeId), lastCheckpoint);
            return Ok(new 
            { 
                message = "Munka lezárult.",
                employeeName = employee.Username,
                duration = lastCheckpoint.CheckOutTime.Value - lastCheckpoint.CheckInTime,
                confirmation = "A megadott azonosítóhoz tartozó felhasználónév a következő: " + employee.Username + ". Megerősíti a munkaba való kilépést?"
            });
        }
    }
}