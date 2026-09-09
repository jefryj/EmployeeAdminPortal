using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminPortal.Models
{
    public class DepartmentDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string DepartmentName { get; set; } = string.Empty;
    }
}
