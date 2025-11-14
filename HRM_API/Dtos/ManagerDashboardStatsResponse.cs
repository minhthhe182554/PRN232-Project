namespace HRM_API.Dtos
{
    public class ManagerDashboardStatsResponse
    {
        public CheckInStatusDto CheckInStatus { get; set; } = new();
        public TeamStatsDto TeamStats { get; set; } = new();
        public RequestStatsDto RequestStats { get; set; } = new();
        public AttendanceStatsDto AttendanceStats { get; set; } = new();
        public List<TopAbsenteeDto> TopAbsentees { get; set; } = new();
    }

    public class CheckInStatusDto
    {
        public bool HasCheckedInToday { get; set; }
        public DateTime? CheckInTime { get; set; }
        public bool IsCheckInOnTime { get; set; }
        public string? CheckInMessage { get; set; }
        public bool HasCheckedOutToday { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public bool IsCheckOutOnTime { get; set; }
        public string? CheckOutMessage { get; set; }
    }

    public class TeamStatsDto
    {
        public int TotalMembers { get; set; }
        public int Level1Count { get; set; }
        public int Level2Count { get; set; }
        public int Level3Count { get; set; }
    }

    public class TopAbsenteeDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int AbsentCount { get; set; }
    }
}

