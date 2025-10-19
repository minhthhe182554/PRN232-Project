using HumanResourceManagement.Models;

namespace HRM_API.Dtos.Dashboard.Policy;

public class UserDto
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Address { get; set; }
    public string ProfileImgUrl { get; set; }
    public decimal Salary { get; set; }
    public int LeaveDaysTaken { get; set; } 
    public bool IsActive { get; set; } = true; 
    public string DepartmentName { get; set; }
    // Collections
    public int LeaveRequestSent { get; set; }
    public int OvertimeRequestSent { get; set; }
    public int AttendancePercentage { get; set; }
}