using System.Collections.Generic;
using System.Threading.Tasks;
using worksystem.DTOs;
using worksystem.Models;
using worksystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace worksystem.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;

        public EmployeeService(AppDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        
        }

        //Az összes dolgozó minden adatának lekérdezése, kivéve a jelszavakat.
        public async Task<List<EmployeeDTO>> GetAllEmployeesByMonth(DateOnly date)
        {
            return await _context.Employees
                .Select(e => new EmployeeDTO
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.FullName,
                    Username = e.Username,
                    UserRole = e.UserRole
                })
                .ToListAsync();
            
        }
        //Egy dolgozó minden adatának lekérdezése, kivéve a jelszavakat megadott ID alapján.
        public async Task<EmployeeDTO> GetEmployeeById(int EmployeeId)
        {
            var employee = await _context.Employees
                .Where(e => e.EmployeeId == EmployeeId)
                .Select(e => new EmployeeDTO
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.FullName,
                    Username = e.Username,
                    UserRole = e.UserRole
                })
                .FirstOrDefaultAsync();

            if (employee == null)
                throw new KeyNotFoundException($"Dolgozó a megadott {EmployeeId} ID számmal nem található.");

            return employee;
            
        }
        //Dolgozók lekérdezése, akik adott jogkörrel rendelkeznek. (Admin/Manager/Employee)
        public async Task<List<EmployeeDTO>> GetEmployeesByRole(UserRole role)
        {
            return await _context.Employees
                .Where(e => e.UserRole == role)
                .Select(e => new EmployeeDTO
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.FullName,
                    Username = e.Username,
                    UserRole = e.UserRole
                })
                .ToListAsync();
        }
        //Egy dolgozó minden adatának lekérdezése, kivéve a jelszavakat megadott felhasználóneve alapján.
        public async Task<EmployeeDTO> GetEmployeeByUsername(string Username)
        {
            if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("A felhasználónév nem lehet üres vagy szóköz.");

            return await _context.Employees
                .Where(e => e.Username.ToLower() == Username.ToLower())
                .Select(e => new EmployeeDTO
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.FullName,
                    Username = e.Username,
                    UserRole = e.UserRole,
                    PasswordHash = e.PasswordHash
                })
                .FirstOrDefaultAsync();

        }
        //Új dolgozó hozzáadása.
        public async Task<bool> AnyAdminExists()
        {
            return await _context.Employees
                .AnyAsync(e => e.UserRole == UserRole.Admin);
        }
        public async Task<EmployeeDTO> CreateEmployee(EmployeeDTO employee)
        {
           if (string.IsNullOrWhiteSpace(employee.FullName))
                throw new ArgumentException("A teljes név nem lehet üres.");

            if (string.IsNullOrWhiteSpace(employee.Username))
                throw new ArgumentException("A felhasználónév nem lehet üres.");

            //Jelszóbevitel ellenőrzése
            _passwordService.ValidatePassword(employee.Password);

            // Ellenőrizzük, hogy létezik-e már ilyen felhasználónév
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Username == employee.Username);

            if (existingEmployee != null)
                throw new ArgumentException($"A(z) '{employee.Username}' felhasználónév már foglalt.");

            var newEmployee = new Employee
            {
                FullName = employee.FullName,
                Username = employee.Username,
                PasswordHash = _passwordService.HashPassword(employee.Password),
                UserRole = employee.UserRole
            };

            _context.Employees.Add(newEmployee);
            await _context.SaveChangesAsync();

            return new EmployeeDTO
            {
                EmployeeId = newEmployee.EmployeeId,
                FullName = newEmployee.FullName,
                Username = newEmployee.Username,
                UserRole = newEmployee.UserRole,
                Password = null
            };
        }
        //Dolgozó teljes nevének módosítása az ID megadása alapján, külön régi és új névvel.
        public async Task<EmployeeDTO> UpdateFullNameByEmployeeId(int EmployeeId, EmployeeDTO employee)
        {
            if (string.IsNullOrWhiteSpace(employee.FullName))
                throw new ArgumentException("A régi teljes név nem lehet üres.");
            if (string.IsNullOrWhiteSpace(employee.NewFullName))
                throw new ArgumentException("Az új teljes név nem lehet üres.");

            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == EmployeeId);

            if (existingEmployee == null)
                throw new KeyNotFoundException($"Dolgozó a megadott {EmployeeId} ID számmal nem található.");

            if (existingEmployee.FullName != employee.FullName)
                throw new ArgumentException("A régi teljes név nem a megadott ID-hoz tartozik.");

            existingEmployee.FullName = employee.NewFullName;
            await _context.SaveChangesAsync();

            return new EmployeeDTO
            {
                EmployeeId = existingEmployee.EmployeeId,
                FullName = existingEmployee.FullName,
                Username = existingEmployee.Username,
                UserRole = existingEmployee.UserRole
            };
        }
        //Dolgozó felhasználónévnek módosítása az ID megadása alapján, külön régi és új felhasználónévvel.
        public async Task<EmployeeDTO> UpdateUsernameByEmployeeId(int EmployeeId, EmployeeDTO employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Username))
                throw new ArgumentException("A régi felhasználónév nem lehet üres.");
            if (string.IsNullOrWhiteSpace(employee.NewUsername))
                throw new ArgumentException("Az új felhasználónév nem lehet üres.");

            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == EmployeeId);

            if (existingEmployee == null)
                throw new KeyNotFoundException($"Dolgozó a megadott {EmployeeId} ID számmal nem található.");

            if (existingEmployee.Username != employee.Username)
                throw new ArgumentException("A régi felhasználónév nem a megadott ID-hoz tartozik.");

            var existingUsername = await _context.Employees
                .AnyAsync(e => e.Username == employee.NewUsername && e.EmployeeId != EmployeeId);
            if (existingUsername)
                throw new ArgumentException($"A(z) '{employee.NewUsername}' felhasználónév már foglalt.");

            existingEmployee.Username = employee.NewUsername;
            await _context.SaveChangesAsync();

            return new EmployeeDTO
            {
                EmployeeId = existingEmployee.EmployeeId,
                FullName = existingEmployee.FullName,
                Username = existingEmployee.Username,
                UserRole = existingEmployee.UserRole,
                Password = null
            };
        }
        //Dolgozó jelszavának módosítása az ID megadása alapján.
        public async Task<EmployeeDTO> UpdatePasswordByEmployeeId(int EmployeeId, EmployeeDTO employee)
        {
             var existingEmployee = await ValidateEmployee(EmployeeId, employee.Username);

            _passwordService.ValidatePassword(employee.Password);

            var passwordHash = _passwordService.HashPassword(employee.Password);
            existingEmployee.PasswordHash = passwordHash;
            await _context.SaveChangesAsync();

            return new EmployeeDTO
            {
                EmployeeId = existingEmployee.EmployeeId,
                FullName = existingEmployee.FullName,
                Username = existingEmployee.Username,
                UserRole = existingEmployee.UserRole,
                Password = null
            };
        }
       //Saját jelszó módosítása
        public async Task<IActionResult> UpdatePassword(PasswordDTO passwordChange)
        {
            var employee = await _context.Employees.FindAsync(passwordChange.EmployeeId);
            if (employee == null)
                return new NotFoundResult();

            if (!_passwordService.VerifyPassword(passwordChange.Password, employee.PasswordHash))
                return new BadRequestResult();

            if (passwordChange.NewPassword != passwordChange.ConfirmNewPassword)
                return new BadRequestResult();

            employee.PasswordHash = _passwordService.HashPassword(passwordChange.NewPassword);
            await _context.SaveChangesAsync();
            return new OkResult();
        }
        //Dolgozó jogkörének módosítása az ID megadása alapján.
        public async Task<EmployeeDTO> UpdateUserRoleByEmployeeId(int EmployeeId, EmployeeDTO employee)
        {
            var existingEmployee = await ValidateEmployee(EmployeeId, employee.Username);

            if (!Enum.IsDefined(typeof(UserRole), employee.UserRole))
                throw new ArgumentException($"Érvénytelen jogosultsági szint: {employee.UserRole}");

            existingEmployee.UserRole = employee.UserRole;
            await _context.SaveChangesAsync();

            return new EmployeeDTO
            {
                EmployeeId = existingEmployee.EmployeeId,
                FullName = existingEmployee.FullName,
                Username = existingEmployee.Username,
                UserRole = existingEmployee.UserRole
            };
        }
        //Dolgozó törlése az ID megadása alapján.
        public async Task DeleteEmployee(int EmployeeId, EmployeeDTO employee)
        {
            var existingEmployee = await ValidateEmployee(EmployeeId, employee.Username);

            if (existingEmployee == null)
                throw new KeyNotFoundException($"Dolgozó a megadott {EmployeeId} ID számmal nem található.");

            if (existingEmployee.Username != employee.Username)
                throw new ArgumentException("A megadott felhasználónév és ID nem tartozik össze.");

            _context.Employees.Remove(existingEmployee);
            await _context.SaveChangesAsync();
        }
        //Segédfüggvény a dolgozó felhasználónevének validálására.
        private async Task<Employee> ValidateEmployee(int EmployeeId, string Username)
        {
            if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("A felhasználónév megerősítésére szolgál.");

            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == EmployeeId);

            if (existingEmployee == null)
                throw new KeyNotFoundException($"Dolgozó a megadott {EmployeeId} ID számmal nem található.");

            if (existingEmployee.Username != Username)
                throw new ArgumentException("A megadott felhasználónév nem egyezik a jelenlegi fiókkal.");

            return existingEmployee;
        }
    }
}
