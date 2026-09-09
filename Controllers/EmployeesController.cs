using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace EmployeeAdminPortal.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private ApplicationDbContext dBcontext;
        private readonly ILogger<EmployeesController> logger;
        public EmployeesController(ApplicationDbContext dBcontext, ILogger<EmployeesController> logger)
        {
            this.dBcontext = dBcontext;
            this.logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            logger.LogInformation("GetAllEmployees endpoint called");

            var employees = await dBcontext.Employees.Include(e => e.Department).Include(e=>e.Project).ToListAsync();
            logger.LogInformation("Retrieved {Count} employees",employees.Count);
            return Ok(employees);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddEmployee(AddEmployeeDto addemp)
        {
            if(addemp == null)
            {
                logger.LogWarning("AddEmployee endpoint called with null AddEmployeeDto");
                return BadRequest("Employee data is required.");
            }
            var department = await dBcontext.Departments.FindAsync(addemp.DepartmentId);
            if (department == null)
            {
                logger.LogWarning("Department with Id: {DepartmentId} not found", addemp.DepartmentId);
                return BadRequest("Invalid department ID.");
            }
            logger.LogInformation("Adding a new employee with Name: {Name}, Email: {Email}", addemp.Name, addemp.Email);
            if (addemp.ProjectId != null)
            {
                var project = await dBcontext.Projects.FindAsync(addemp.ProjectId);

                if (project == null)
                {
                    logger.LogWarning(
                        "Invalid ProjectId: {ProjectId}",addemp.ProjectId);

                    return BadRequest("Invalid ProjectId.");
                }

                project.ProjectMembersCount++;
            }
            var emp = new Employee()
            {
                Id = Guid.NewGuid(),
                Name = addemp.Name,
                Email = addemp.Email,
                Phone = addemp.Phone,
                Salary = addemp.Salary,
                DepartmentId = addemp.DepartmentId,
                ProjectId = addemp.ProjectId,
                Role = addemp.Role

            };
            var hasher = new PasswordHasher<Employee>();
            emp.PasswordHash = hasher.HashPassword(emp, addemp.Password);
            await dBcontext.Employees.AddAsync(emp);
            await dBcontext.SaveChangesAsync();
            logger.LogInformation("Employee added successfully with Id: {Id}", emp.Id);

            return CreatedAtAction(nameof(GetAllEmployeesById), new { id = emp.Id }, emp);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllEmployeesById(Guid id)
        {
            logger.LogInformation("GetAllEmployeesById endpoint called with Id: {Id}", id);
            var employee = await dBcontext.Employees.Include(e => e.Department).Include(e => e.Project).FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                logger.LogWarning("Employee with Id: {Id} not found", id);
                return NotFound();
                
            }
            logger.LogInformation("Employee with Id: {Id} retrieved successfully", id);
            return Ok(employee);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateEmployee(Guid id, UpdateEmployeeDto updateEmp)
        {   
            if(updateEmp == null)
            {
                logger.LogWarning("UpdateEmployee endpoint called with null UpdateEmployeeDto");
                return BadRequest("Employee data is required.");
            }
            var department = await dBcontext.Departments.FindAsync(updateEmp.DepartmentId);

            if (department == null)
            {
                logger.LogWarning("Invalid DepartmentId: {DepartmentId}",updateEmp.DepartmentId);

                return BadRequest("Invalid DepartmentId.");
            }
            if (updateEmp.ProjectId != null)
            {
                var project =await dBcontext.Projects.FindAsync(updateEmp.ProjectId);

                if (project == null)
                {
                    logger.LogWarning("Invalid ProjectId: {ProjectId}",updateEmp.ProjectId);

                    return BadRequest("Invalid ProjectId.");
                }
            }
            logger.LogInformation("updateEmployee endpoint called with Id: {Id}", id);

            var employee = await dBcontext.Employees.FindAsync(id);

            if (employee == null)
            {
                logger.LogWarning("Employee with Id: {Id} not found", id);
                return NotFound();
            }
            var oldProjectId = employee.ProjectId;

            
            employee.Name = updateEmp.Name;
            employee.Email = updateEmp.Email;
            employee.Phone = updateEmp.Phone;
            employee.Salary = updateEmp.Salary;
            employee.DepartmentId = updateEmp.DepartmentId;
            employee.ProjectId = updateEmp.ProjectId;

            if (oldProjectId != updateEmp.ProjectId)
            {
                if (oldProjectId != null)
                {
                    var oldProject =
                        await dBcontext.Projects.FindAsync(oldProjectId);

                    if (oldProject != null)
                    {
                        oldProject.ProjectMembersCount--;
                    }
                }

                if (updateEmp.ProjectId != null)
                {
                    var newProject =
                        await dBcontext.Projects.FindAsync(updateEmp.ProjectId);

                    if (newProject != null)
                    {
                        newProject.ProjectMembersCount++;
                    }
                }
            }

            await dBcontext.SaveChangesAsync();
            logger.LogInformation("Employee with Id: {Id} updated successfully", id);
            return Ok(employee);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {   
            logger.LogInformation("DeleteEmployee endpoint called with Id: {Id}", id);
            var employee=await dBcontext.Employees.FindAsync(id);
            if (employee == null)
            {
                logger.LogWarning("Employee with Id: {Id} not found", id);
                return NotFound();
            }
            if (employee.ProjectId != null)
            {
                var project =
                    await dBcontext.Projects.FindAsync(employee.ProjectId);

                if (project != null)
                {
                    project.ProjectMembersCount--;
                }
            }
            dBcontext.Employees.Remove(employee);
            await dBcontext.SaveChangesAsync();
            logger.LogInformation("Employee with Id: {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
