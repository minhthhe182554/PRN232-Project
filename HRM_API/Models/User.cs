using HRM_API.Models.Enums;

namespace HRM_API.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public Role Role { get; set; }
        public int Level { get; set; } = 1;
        public required string FullName { get; set; }
        public required string Address { get; set; }
        public string? ProfileImgUrl { get; set; }
        public int AnnualLeaveDays { get; set; } = 12;
        public bool IsActive { get; set; } = true; // soft-delete
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public Department? ManagedDepartment { get; set; }
        // Collections
        public ICollection<Request> Requests { get; set; } = new List<Request>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<SharedFile> SharedFiles { get; set; } = new List<SharedFile>();
    }
}
