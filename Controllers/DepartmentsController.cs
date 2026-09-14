using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;


namespace EmployeeAdminPortal.Controllers
{   
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {   
        private static readonly List<string> cacheKeys = new();
        private readonly IMemoryCache cache;
        private readonly ApplicationDbContext dBcontext;

        public DepartmentsController(ApplicationDbContext dBcontext, IMemoryCache cache)
        {
            this.dBcontext = dBcontext;
            this.cache = cache;
        }

        private void ClearDepartmentCache()
            {
                foreach (var key in cacheKeys)
                {
                    cache.Remove(key);
                }

                cacheKeys.Clear();
            }
        [HttpGet]
        public async Task<IActionResult> GetAllDepartments([FromQuery] DepartmentSearchDto searchDto)
        {
            IQueryable<Department> query = dBcontext.Departments;

            if (!string.IsNullOrWhiteSpace(searchDto.Search))
            {
                query = query.Where(d => d.DepartmentName.Contains(searchDto.Search));
            }

            string cacheKey = $"departments-{searchDto.Search}-{searchDto.PageNumber}-{searchDto.PageSize}";

            List<Department>? departments;

            bool foundInCache = cache.TryGetValue(cacheKey, out departments);

            if (foundInCache == false)
            {
                departments = await query.OrderBy(d => d.Id).Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize).ToListAsync();

                cache.Set(cacheKey, departments, TimeSpan.FromMinutes(5));

                if (!cacheKeys.Contains(cacheKey))
                {
                    cacheKeys.Add(cacheKey);
                }
            }

            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var department = await dBcontext.Departments.FindAsync(id);
            if(department == null)
            {
                throw new KeyNotFoundException("Department not found");
            }
            return Ok(department);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]

        public async Task<IActionResult> AddDepartment(DepartmentDto dto)
        {
            if(dto == null)
            {
                throw new ArgumentException("Department data is required.");
            }
            bool departmentExists = await dBcontext.Departments.AnyAsync(d => d.DepartmentName == dto.DepartmentName);

            if (departmentExists)
            {
                throw new ArgumentException("Department already exists.");
            }
            var department = new Department
            {
                DepartmentName = dto.DepartmentName
            };

            await dBcontext.Departments.AddAsync(department);
            await dBcontext.SaveChangesAsync();
            var auditLog = new AuditLog
            {
                UserName = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                Action = "Create",
                EntityName = "Department",
                Details = $"Department {department.DepartmentName} was created",
                CreatedAt = DateTime.UtcNow
            };

            await dBcontext.AuditLogs.AddAsync(auditLog);
            await dBcontext.SaveChangesAsync();
            ClearDepartmentCache();
            return CreatedAtAction(nameof(GetDepartmentById), new { id = department.Id }, department);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateDepartment(int id, DepartmentDto dto)
        {   
            if(dto == null)
            {
                throw new ArgumentException("Department data is required.");
            }
            var department = await dBcontext.Departments.FindAsync(id);
            if (department == null)
            {
                throw new KeyNotFoundException("Department not found");
            }
            bool departmentExists = await dBcontext.Departments.AnyAsync(d =>d.Id != id && d.DepartmentName == dto.DepartmentName);

            if (departmentExists)
            {
                throw new ArgumentException("Department already exists.");
            }
            department.DepartmentName = dto.DepartmentName;
            await dBcontext.SaveChangesAsync();
            var auditLog = new AuditLog
            {
                UserName = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                Action = "Update",
                EntityName = "Department",
                Details = $"Department {department.DepartmentName} was updated",
                CreatedAt = DateTime.UtcNow
            };
            await dBcontext.AuditLogs.AddAsync(auditLog);
            await dBcontext.SaveChangesAsync();
            ClearDepartmentCache();
            return Ok(department);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await dBcontext.Departments.FindAsync(id);
            if (department == null)
            {
                throw new KeyNotFoundException("Department not found");
            }
            dBcontext.Departments.Remove(department);
            await dBcontext.SaveChangesAsync();
            var auditLog = new AuditLog
            {
                UserName = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                Action = "Delete",
                EntityName = "Department",
                Details = $"Department {department.DepartmentName} was deleted",
                CreatedAt = DateTime.UtcNow
            };
            await dBcontext.AuditLogs.AddAsync(auditLog);
            await dBcontext.SaveChangesAsync();
            ClearDepartmentCache();
            return NoContent();
        }
    }
}
