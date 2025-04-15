using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using worksystem.Services;
using worksystem.DTOs;
using worksystem.Models;
using AutoMapper;
using worksystem.Data;

namespace worksystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IJwtTokenService _jwtService;
        private readonly ILogger<AuthController> _logger;
        private readonly PasswordService _passwordService;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public AuthController(
            IEmployeeService employeeService,
            IJwtTokenService jwtService,
            ILogger<AuthController> logger,
            PasswordService passwordService,
            IMapper mapper,
            AppDbContext context)
        {
            _employeeService = employeeService;
            _jwtService = jwtService;
            _logger = logger;
            _passwordService = passwordService;
            _mapper = mapper;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Console.WriteLine($"LOGIN DEBUG:");
            Console.WriteLine($"Request.Username: {request?.Username}");
            Console.WriteLine($"Request.Password: {request?.Password}");

            var employee = await _employeeService.GetEmployeeByUsername(request?.Username);

            Console.WriteLine($"DB Username: {employee?.Username}");
            Console.WriteLine($"DB Hash: {employee?.PasswordHash}");
            if (employee != null)
            {
                Console.WriteLine($"Jelszó ellenőrzése: {_passwordService.VerifyPassword(request.Password, employee.PasswordHash)}");
            }

            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Érvénytelen bejelentkezési adat");
            }
                    if (employee == null || string.IsNullOrEmpty(employee.PasswordHash))
                    {
                        _logger.LogWarning($"Érvénytelen bejelentkezés {request.Username}");
                        return Unauthorized("Rossz jelszó vagy felhasználónév");
                    }
                    
                    _logger.LogDebug($"Jelszó ellenőrzése: '{request.Password}' with hash: '{employee.PasswordHash}'");
                    
                    if (!_passwordService.VerifyPassword(request.Password, employee.PasswordHash))
                    {
                        _logger.LogWarning($"Rossz jelszó {request.Username}");
                        return Unauthorized("Rossz jelszó vagy felhasználónév");
                    }

                    var token = _jwtService.GenerateToken(_mapper.Map<Employee>(employee));
                    
                    _logger.LogInformation($"Sikeres bejelentkezés: {request.Username}");

                    return Ok(new { 
                        Token = token,
                        EmployeeId = employee.Id,
                        FullName = employee.FullName,
                        UserRole = employee.UserRole
                    });
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest request)
        {
            if (await _employeeService.GetEmployeeByUsername(request.Username) != null)
            {
                return BadRequest("A megadott felhasználónév már foglalt");
            }

            _passwordService.ValidatePassword(request.Password);

            var newEmployee = _mapper.Map<Employee>(request);
            newEmployee.PasswordHash = _passwordService.HashPassword(request.Password);
            
            var createdAdmin = await _employeeService.CreateEmployee(_mapper.Map<EmployeeDTO>(newEmployee));

            _logger.LogInformation($"Admin létrehozva: {request.Username}");

            _passwordService.ValidatePassword(request.Password);

            var tokenEmployee = new Employee 
            {
                EmployeeId = createdAdmin.EmployeeId,
                FullName = createdAdmin.FullName,
                Username = createdAdmin.Username,
                UserRole = createdAdmin.UserRole,
                PasswordHash = createdAdmin.PasswordHash
            };

            return Ok(new 
            {  
                Message = "Első admin sikeresen hozzáadva!",
                Id = createdAdmin.EmployeeId,
                FullName = createdAdmin.FullName,
                Token = _jwtService.GenerateToken(tokenEmployee)
            });
        }

        [HttpPost("create-first-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateFirstAdmin([FromBody] RegisterRequest request)
        {
            try 
            {
                if (await _employeeService.AnyAdminExists())
                {
                    return BadRequest("Egy admin már létezik.");
                }

                _passwordService.ValidatePassword(request.Password);
                
                var employeeDto = _mapper.Map<EmployeeDTO>(request);
                employeeDto.UserRole = UserRole.Admin;
                employeeDto.EmployeeId = 100;
                employeeDto.Password = request.Password;
                
                var employee = await _employeeService.CreateEmployee(employeeDto);
                
                _logger.LogInformation($"Első admin sikeresen hozzáadva: {employee.Username}");

                return Ok(new 
                {
                    Id = employee.EmployeeId,
                    Username = employee.Username,
                    Role = employee.UserRole,
                    Token = _jwtService.GenerateToken(_mapper.Map<Employee>(employee))
                });
            }
            catch (InvalidOperationException ex) 
            {
                _logger.LogError(ex, "Jelszó ellenőrzése sikertelen");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba az első admin létrehozása során");
                return StatusCode(500, "Belső szerverhiba");
            }
        }
    }
}