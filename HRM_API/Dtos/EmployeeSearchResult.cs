namespace HRM_API.Dtos
{
    public class EmployeeSearchResult
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public int Level { get; set; }
        public string? DepartmentName { get; set; }
    }
}

