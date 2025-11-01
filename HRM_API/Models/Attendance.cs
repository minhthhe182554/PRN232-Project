using HRM_API.Models.Enums;

namespace HRM_API.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int UserId { get; set; } 
        public User User { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public AttendanceStatus Status { get; set; }
        public TimeSpan? WorkingHours => CheckOut?.Subtract(CheckIn);
    }
}
