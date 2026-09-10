using EmployeeAdminPortal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext dBcontext;
        private readonly ILogger<ReportsController> logger;

        public ReportsController(ApplicationDbContext dBcontext, ILogger<ReportsController> logger)
        {
            this.dBcontext = dBcontext;
            this.logger = logger;
        }

        [HttpGet("department-summary")]
        public async Task<IActionResult> GetDepartmentSummary()
        {
            logger.LogInformation("GetDepartmentSummary endpoint called");

            var report = await dBcontext.Departments
                .Select(d => new
                {
                    d.DepartmentName,
                    EmployeeCount = dBcontext.Employees.Count(e => e.DepartmentId == d.Id),
                    AverageSalary = dBcontext.Employees.Where(e => e.DepartmentId == d.Id).Average(e => (decimal?)e.Salary) ?? 0
                }).ToListAsync();

            logger.LogInformation("Retrieved {Count} department summaries", report.Count);

            return Ok(report);
        }

        [HttpGet("project-summary")]
        public async Task<IActionResult> GetProjectSummary()
        {
            logger.LogInformation("GetProjectSummary endpoint called");

            var report = await dBcontext.Projects
                .Select(p => new
                {
                    p.ProjectName,
                    EmployeeCount = dBcontext.Employees.Count(e => e.ProjectId == p.Id),
                    AverageSalary = dBcontext.Employees.Where(e => e.ProjectId == p.Id).Average(e => (decimal?)e.Salary) ?? 0
                }).ToListAsync();

            logger.LogInformation("Retrieved {Count} project summaries", report.Count);

            return Ok(report);
        }
    }
    
}