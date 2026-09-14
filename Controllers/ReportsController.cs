using EmployeeAdminPortal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext dBcontext;
        private readonly ILogger<ReportsController> logger;
        private readonly IMemoryCache cache;

        public ReportsController(ApplicationDbContext dBcontext, ILogger<ReportsController> logger, IMemoryCache cache)
        {
            this.dBcontext = dBcontext;
            this.logger = logger;
            this.cache = cache;
        }

        [HttpGet("department-summary")]
        public async Task<IActionResult> GetDepartmentSummary()
        {
            logger.LogInformation("GetDepartmentSummary endpoint called");

            const string cacheKey = "department-summary";

        if (!cache.TryGetValue(cacheKey, out object? report))
        {
            logger.LogInformation("Department Summary CACHE MISS");

            report = await dBcontext.Departments
                .Select(d => new
                {
                    d.DepartmentName,
                    EmployeeCount = dBcontext.Employees.Count(e => e.DepartmentId == d.Id),
                    AverageSalary = dBcontext.Employees.Where(e => e.DepartmentId == d.Id).Average(e => (decimal?)e.Salary) ?? 0
                }).ToListAsync();

            cache.Set(cacheKey, report, TimeSpan.FromMinutes(5));
        }
        else
        {
            logger.LogInformation("Department Summary CACHE HIT");
        }

        return Ok(report);
        }

        [HttpGet("project-summary")]
        public async Task<IActionResult> GetProjectSummary()
        {
            logger.LogInformation("GetProjectSummary endpoint called");

            const string cacheKey = "project-summary";

        if (!cache.TryGetValue(cacheKey, out object? report))
        {
            logger.LogInformation("Project Summary CACHE MISS");

            report = await dBcontext.Projects
                .Select(p => new
                {
                    p.ProjectName,
                    EmployeeCount = dBcontext.Employees.Count(e => e.ProjectId == p.Id),
                    AverageSalary = dBcontext.Employees.Where(e => e.ProjectId == p.Id).Average(e => (decimal?)e.Salary) ?? 0
                }).ToListAsync();

            cache.Set(cacheKey, report, TimeSpan.FromMinutes(5));
        }
        else
        {
            logger.LogInformation("Project Summary CACHE HIT");
        }

        return Ok(report);

        }
    }
    
}