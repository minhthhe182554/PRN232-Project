using Microsoft.AspNetCore.Identity;

namespace HumanResourceManagement.Models
{
	public class User : IdentityUser
	{
		public string FullName { get; set; }
		public string Address { get; set; }

		// Avatar mặc định (Firebase link)
		public string ProfileImgUrl { get; set; } = "https://firebasestorage.googleapis.com/default-avatar.png";
		public decimal Salary { get; set; }     
		public int AnnualLeaveDays { get; set; } = 12; 

		public bool IsActive { get; set; } = true; // soft-delete

		public int? DepartmentId { get; set; }
		public Department Department { get; set; }

		// Nếu user này là Manager -> quản lý 1 department
		public Department ManagedDepartment { get; set; }
		// Collections
		public ICollection<LeaveRequest> LeaveRequests { get; set; }
		public ICollection<OvertimeRequest> OvertimeRequests { get; set; }
		public ICollection<Attendance> Attendances { get; set; }
		public ICollection<Notification> Notifications { get; set; }
	}
}
