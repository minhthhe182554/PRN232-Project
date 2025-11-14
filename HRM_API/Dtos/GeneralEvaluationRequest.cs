using System.Text.Json.Serialization;

namespace HRM_API.Dtos
{
    public class GeneralEvaluationRequest
    {
        [JsonPropertyName("evaluationPeriod")]
        public string EvaluationPeriod { get; set; } = "30 days";

        [JsonPropertyName("totalEmployees")]
        public int TotalEmployees { get; set; }

        [JsonPropertyName("employees")]
        public List<EmployeePerformanceData> Employees { get; set; } = new();
    }

    public class EmployeePerformanceData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("department")]
        public string? Department { get; set; }

        [JsonPropertyName("attendance")]
        public AttendanceData Attendance { get; set; } = new();

        [JsonPropertyName("requests")]
        public RequestData Requests { get; set; } = new();
    }

    public class AttendanceData
    {
        [JsonPropertyName("totalWorkingDays")]
        public int TotalWorkingDays { get; set; }

        [JsonPropertyName("presentDays")]
        public int PresentDays { get; set; }

        [JsonPropertyName("lateDays")]
        public int LateDays { get; set; }

        [JsonPropertyName("leaveEarlyDays")]
        public int LeaveEarlyDays { get; set; }

        [JsonPropertyName("absentDays")]
        public int AbsentDays { get; set; }

        [JsonPropertyName("averageWorkingHours")]
        public double AverageWorkingHours { get; set; }

        [JsonPropertyName("attendanceRate")]
        public double AttendanceRate { get; set; }
    }

    public class RequestData
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("approved")]
        public int Approved { get; set; }

        [JsonPropertyName("rejected")]
        public int Rejected { get; set; }

        [JsonPropertyName("pending")]
        public int Pending { get; set; }

        [JsonPropertyName("leaveRequests")]
        public int LeaveRequests { get; set; }

        [JsonPropertyName("resignationRequests")]
        public int ResignationRequests { get; set; }

        [JsonPropertyName("overtimeRequests")]
        public int OvertimeRequests { get; set; }
    }
}

