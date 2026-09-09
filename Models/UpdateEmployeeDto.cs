using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminPortal.Models
{
    public class UpdateEmployeeDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Range(1000, 1000000)]

        public decimal Salary { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        public int? ProjectId { get; set; }
    }
}
