namespace HRM_Client.Models
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
        public double PendingPercentage { get; set; }
        public double ApprovedPercentage { get; set; }
        public double RejectedPercentage { get; set; }
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
        public double PresentPercentage { get; set; }
        public double AbsentPercentage { get; set; }
        public double LatePercentage { get; set; }
        public double OnLeavePercentage { get; set; }
    }
}

