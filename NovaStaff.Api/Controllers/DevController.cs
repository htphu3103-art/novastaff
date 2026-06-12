using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NovaStaff.API.Controllers
{
    [ApiController]
    [Route("api/dev")]
    public class DevController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public DevController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpGet("token")]
        public IActionResult GetToken()
        {
            var fakeUser = new User
            {
                UserID = 1,
                Username = "test",
                Role = UserRole.Admin
            };

            var token = _tokenService.GenerateAccessToken(fakeUser);

            return Ok(token);
        }

        [Authorize]
        [HttpGet("private")]
        public IActionResult Private()
        {
            return Ok("Authorized");
        }

        [HttpPost("seed-admin")]
        public async Task<IActionResult> SeedAdmin([FromServices] NovaStaff.DataLayers.AppDbContext dbContext)
        {
            if (await dbContext.Users.AnyAsync(u => u.Username == "admin"))
            {
                // Trả về đúng thông tin login (dùng email để đăng nhập)
                var existing = await dbContext.Users
                    .Include(u => u.Employee)
                    .FirstAsync(u => u.Username == "admin");

                return Ok(new
                {
                    Message = "Admin already exists!",
                    LoginEmail = existing.Employee!.Email,
                    Password = "(không đổi – dùng mật khẩu đã set)",
                    Note = "Đăng nhập tại POST /api/auth/login với field 'username' = LoginEmail"
                });
            }

            const string adminEmail = "admin@novastaff.local";
            const string adminPassword = "Admin@123";

            var adminEmployee = new Employee
            {
                EmployeeCode = "EMP-ADMIN",
                FullName = "System Administrator",
                Gender = GenderType.Other,
                Email = adminEmail,
                Status = EmployeeStatus.Active,
                BaseSalary = 0,
                JoinDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            dbContext.Employees.Add(adminEmployee);
            await dbContext.SaveChangesAsync();

            var adminUser = new User
            {
                EmployeeID = adminEmployee.EmployeeID,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = UserRole.Admin,
                IsActive = true
            };
            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Admin created successfully!",
                LoginEmail = adminEmail,
                Password = adminPassword,
                Note = "Đăng nhập tại POST /api/auth/login với field 'username' = LoginEmail",
                EmployeeId = adminEmployee.EmployeeID
            });
        }
    }
}
