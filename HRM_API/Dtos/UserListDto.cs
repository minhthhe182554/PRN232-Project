using HRM_API.Models;
using HRM_API.Models.Enums;

namespace HRM_API.Dtos
{
    public class UserListDto
    {
        public int Id { get; set; }
        public Role Role { get; set; }
        public int Level { get; set; } = 1;
        public required string FullName { get; set; }
        public bool IsActive { get; set; } = true; // soft-delete
        public string? DepartmentName { get; set; }
        public string? ManagedDepartmentName { get; set; }
        public AttendanceStatus? TodayAttendanceStatus { get; set; }
    }
}

