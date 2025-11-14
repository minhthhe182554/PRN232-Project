namespace HRM_API.Dtos
{
    public class DashboardStatsResponse
    {
        public RequestStatsDto RequestStats { get; set; } = new();
        public AttendanceStatsDto AttendanceStats { get; set; } = new();
    }

    public class RequestStatsDto
    {
        public RequestPeriodStats Today { get; set; } = new();
        public RequestPeriodStats ThisWeek { get; set; } = new();
    }

    public class RequestPeriodStats
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public double PendingPercentage => Total > 0 ? Math.Round((double)Pending / Total * 100, 1) : 0;
        public double ApprovedPercentage => Total > 0 ? Math.Round((double)Approved / Total * 100, 1) : 0;
        public double RejectedPercentage => Total > 0 ? Math.Round((double)Rejected / Total * 100, 1) : 0;
    }

    public class AttendanceStatsDto
    {
        public AttendancePeriodStats Today { get; set; } = new();
        public AttendancePeriodStats ThisWeek { get; set; } = new();
    }

    public class AttendancePeriodStats
    {
        public int Total { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int OnLeave { get; set; }
        public double PresentPercentage => Total > 0 ? Math.Round((double)Present / Total * 100, 1) : 0;
        public double AbsentPercentage => Total > 0 ? Math.Round((double)Absent / Total * 100, 1) : 0;
        public double LatePercentage => Total > 0 ? Math.Round((double)Late / Total * 100, 1) : 0;
        public double OnLeavePercentage => Total > 0 ? Math.Round((double)OnLeave / Total * 100, 1) : 0;
    }
}

