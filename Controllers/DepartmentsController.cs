using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace EmployeeAdminPortal.Controllers
{   
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext dBcontext;

        public DepartmentsController(ApplicationDbContext dBcontext)
        {
            this.dBcontext = dBcontext;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllDepartments([FromQuery] DepartmentSearchDto searchDto)
        {
            IQueryable<Department> query = dBcontext.Departments;

            if (!string.IsNullOrWhiteSpace(searchDto.Search))
            {
                query = query.Where(d => d.DepartmentName.Contains(searchDto.Search));
            }

            var departments = await query
                .OrderBy(d => d.Id)
                .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var department = await dBcontext.Departments.FindAsync(id);
            if(department == null)
            {
                return NotFound();
            }
            return Ok(department);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]

        public async Task<IActionResult> AddDepartment(DepartmentDto dto)
        {
            if(dto == null)
            {
                return BadRequest("Department data is required.");
            }
            bool departmentExists = await dBcontext.Departments.AnyAsync(d => d.DepartmentName == dto.DepartmentName);

            if (departmentExists)
            {
                return BadRequest("Department already exists.");
            }
            var department = new Department
            {
                DepartmentName = dto.DepartmentName
            };

            await dBcontext.Departments.AddAsync(department);
            await dBcontext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDepartmentById), new { id = department.Id }, department);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateDepartment(int id, DepartmentDto dto)
        {   
            if(dto == null)
            {
                return BadRequest("Department data is required.");
            }
            var department = await dBcontext.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            bool departmentExists = await dBcontext.Departments.AnyAsync(d =>d.Id != id && d.DepartmentName == dto.DepartmentName);

            if (departmentExists)
            {
                return BadRequest("Department already exists.");
            }
            department.DepartmentName = dto.DepartmentName;
            await dBcontext.SaveChangesAsync();
            return Ok(department);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await dBcontext.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            dBcontext.Departments.Remove(department);
            await dBcontext.SaveChangesAsync();
            return NoContent();
        }
    }
}
