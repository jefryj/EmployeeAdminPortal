

namespace EmployeeAdminPortal.Models.Entities
{
    public class Project
    {
        public int Id { get; set; }

        public required string ProjectName { get; set; }

        public int ProjectMembersCount { get; set; }
    }
}