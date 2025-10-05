namespace HumanResourceManagement.Models
{
	public class Attendance
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public User User { get; set; }
		public DateTime CheckIn { get; set; }
		public DateTime? CheckOut { get; set; }
		public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

		// Computed property để tính working hours
		public TimeSpan? WorkingHours => CheckOut?.Subtract(CheckIn);
	}

	public enum AttendanceStatus
	{
		Present = 1,    // Có mặt đúng giờ
		Late = 2,       // Đi muộn  
		Absent = 3,     // Vắng mặt
		HalfDay = 4     // Nghỉ nửa ngày
	}
}
