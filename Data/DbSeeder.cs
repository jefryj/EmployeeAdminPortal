using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(ApplicationDbContext context)
        {
            if (await context.Employees.AnyAsync(e => e.Role == "Admin"))
            {
                Console.WriteLine("Admin already exists");
                return;
            }

            var admin = new Employee
            {
                Id = Guid.NewGuid(),
                Name = "System Admin",
                Email = "admin@company.com",
                Phone = "9999999999",
                Salary = 0,
                DepartmentId = 1,
                Role = "Admin"
            };

            admin.PasswordHash =new PasswordHasher<Employee>().HashPassword(admin, "Admin123!");

            context.Employees.Add(admin);

            await context.SaveChangesAsync();
        }
    }
}