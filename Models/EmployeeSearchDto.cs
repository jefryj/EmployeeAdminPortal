public class EmployeeSearchDto
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 4;

    public string? SortBy { get; set; } = "Id";
    public bool Descending { get; set; } = false;
}