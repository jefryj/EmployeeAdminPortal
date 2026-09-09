using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration configuration;

        public AuthController(ApplicationDbContext dbContext,IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var employee = await dbContext.Employees.FirstOrDefaultAsync(e => e.Email == request.Email);
            if (employee == null)
            {
                return BadRequest("Invalid email or password");
            }
            var passwordVerificationResult = new PasswordHasher<Employee>().VerifyHashedPassword(employee, employee.PasswordHash, request.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return BadRequest("Invalid email or password");
            }

            var token = CreateToken(employee);
            return Ok(token);
        }
        private string CreateToken(Employee employee)
        {
            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier,employee.Id.ToString()),

            new Claim(ClaimTypes.Email,employee.Email),

            new Claim(ClaimTypes.Role,employee.Role)
            };

            var key= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Token"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
            );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}