using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace EmployeeAdminPortal.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private static readonly List<string> cacheKeys = new();

        private readonly IMemoryCache cache;
        private readonly ApplicationDbContext dBcontext;

        public ProjectsController(ApplicationDbContext dBcontext, IMemoryCache cache)
        {
            this.dBcontext = dBcontext;
            this.cache = cache;
        }
        private void ClearProjectCache()
        {
            foreach (var key in cacheKeys)
            {
                cache.Remove(key);
            }
            cacheKeys.Clear();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProjects([FromQuery] ProjectSearchDto searchDto)
        {
            IQueryable<Project> query = dBcontext.Projects;

            if (!string.IsNullOrWhiteSpace(searchDto.Search))
            {
                query = query.Where(p => p.ProjectName.Contains(searchDto.Search));
            }

            string cacheKey = $"projects-{searchDto.Search}-{searchDto.PageNumber}-{searchDto.PageSize}";

            List<Project>? projects;

            bool foundInCache = cache.TryGetValue(cacheKey, out projects);

            if (!foundInCache)
            {
                projects = await query.OrderBy(p => p.Id).Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize).ToListAsync();

                cache.Set(cacheKey, projects, TimeSpan.FromMinutes(5));

                if (!cacheKeys.Contains(cacheKey))
                {
                    cacheKeys.Add(cacheKey);
                }
            }

            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var project = await dBcontext.Projects.FindAsync(id);

            if (project == null)
            {
                throw new KeyNotFoundException("Project not found");
            }

            return Ok(project);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddProject(ProjectDto dto)
        {   
            if(dto == null)
            {
                throw new ArgumentException("Project data is required.");
            }
            bool projectExists = await dBcontext.Projects.AnyAsync(p => p.ProjectName == dto.ProjectName);

            if (projectExists)
            {
                throw new ArgumentException("Project already exists.");
            }
            var project = new Project
            {
                
                ProjectName = dto.ProjectName,
                ProjectMembersCount = 0
            };

            await dBcontext.Projects.AddAsync(project);

            await dBcontext.SaveChangesAsync();

            var auditLog = new AuditLog
            {
                UserName = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                Action = "Create",
                EntityName = "Project",
                Details = $"Project {project.ProjectName} was created",
                CreatedAt = DateTime.UtcNow
            };

            await dBcontext.AuditLogs.AddAsync(auditLog);
            await dBcontext.SaveChangesAsync();
            ClearProjectCache();

            return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectDto dto)
        {   
            if(dto == null)
            {
                throw new ArgumentException("Project data is required.");
            }
            var project = await dBcontext.Projects.FindAsync(id);

            if (project == null)
            {
                throw new KeyNotFoundException("Project not found");
            }
            bool projectExists = await dBcontext.Projects.AnyAsync(p => p.Id != id && p.ProjectName == dto.ProjectName);

            if (projectExists)
            {
                throw new ArgumentException("Project already exists.");
            }

            project.ProjectName = dto.ProjectName;

            await dBcontext.SaveChangesAsync();
            var auditLog = new AuditLog
            {
                UserName = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                Action = "Update",
                EntityName = "Project",
                Details = $"Project {project.ProjectName} was updated",
                CreatedAt = DateTime.UtcNow
            };
            await dBcontext.AuditLogs.AddAsync(auditLog);
            await dBcontext.SaveChangesAsync();
            ClearProjectCache();

            return Ok(project);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await dBcontext.Projects.FindAsync(id);

            if (project == null)
            {
                throw new KeyNotFoundException("Project not found");
            }

            dBcontext.Projects.Remove(project);

            await dBcontext.SaveChangesAsync();
            var auditLog = new AuditLog
            {
                UserName = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                Action = "Delete",
                EntityName = "Project",
                Details = $"Project {project.ProjectName} was deleted",
                CreatedAt = DateTime.UtcNow
            };
            await dBcontext.AuditLogs.AddAsync(auditLog);
            await dBcontext.SaveChangesAsync();

            ClearProjectCache();
            return NoContent();
        }
    }
}
