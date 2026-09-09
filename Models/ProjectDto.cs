using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminPortal.Models
{
    public class ProjectDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string ProjectName { get; set; } = string.Empty;
    }
}