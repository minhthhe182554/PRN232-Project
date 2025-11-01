namespace HRM_Client.Models
{
    public class PolicyResponse
    {
        public int Id { get; set; }
        public TimeSpan WorkStartTime { get; set; }
        public TimeSpan WorkEndTime { get; set; }
        public int LateThresholdMinutes { get; set; }
        public decimal LateDeductionPercent { get; set; }
        public int LeaveEarlyThresholdMinutes { get; set; }
        public decimal LeaveEarlyDeductionPercent { get; set; }
        public int MonthlyOvertimeHoursLimit { get; set; }
        public int AnnualLeaveMaxDays { get; set; }
    }
}
