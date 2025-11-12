namespace HRM_Client.Models
{
    public class UserListDto
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public int Level { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? DepartmentName { get; set; }
        public string? ManagedDepartmentName { get; set; }
    }
}

