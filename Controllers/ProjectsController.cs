using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeAdminPortal.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {

        private readonly ApplicationDbContext dBcontext;

        public ProjectsController(ApplicationDbContext dBcontext)
        {
            this.dBcontext = dBcontext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            return Ok(await dBcontext.Projects.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var project = await dBcontext.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddProject(ProjectDto dto)
        {   
            if(dto == null)
            {
                return BadRequest("Project data is required.");
            }
            bool projectExists = await dBcontext.Projects.AnyAsync(p => p.ProjectName == dto.ProjectName);

            if (projectExists)
            {
                return BadRequest("Project already exists.");
            }
            var project = new Project
            {
                
                ProjectName = dto.ProjectName,
                ProjectMembersCount = 0
            };

            await dBcontext.Projects.AddAsync(project);

            await dBcontext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectDto dto)
        {   
            if(dto == null)
            {
                return BadRequest("Project data is required.");
            }
            var project = await dBcontext.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }
            bool projectExists = await dBcontext.Projects.AnyAsync(p => p.Id != id && p.ProjectName == dto.ProjectName);

            if (projectExists)
            {
                return BadRequest("Project already exists.");
            }

            project.ProjectName = dto.ProjectName;

            await dBcontext.SaveChangesAsync();

            return Ok(project);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await dBcontext.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            dBcontext.Projects.Remove(project);

            await dBcontext.SaveChangesAsync();

            return NoContent();
        }
    }
}
