namespace HRM_Client.Models
{
    public class AttendanceHistoryResponse
    {
        public List<AttendanceRecordDto> Records { get; set; } = new();
        public string Period { get; set; } = string.Empty;
    }

    public class AttendanceRecordDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string Status { get; set; } = string.Empty;
        public TimeSpan? WorkingHours { get; set; }
    }
}

