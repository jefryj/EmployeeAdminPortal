using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EmployeeAdminPortal.Models.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }

        public string Phone { get; set; } = string.Empty;

        public decimal Salary { get; set; }

        public int DepartmentId { get; set; }

        public Department Department { get; set; } = null!;
        public int? ProjectId { get; set; }

        public Project? Project { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Employee";

    }
}
