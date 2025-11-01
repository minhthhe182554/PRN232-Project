namespace HRM_API.Models
{
    public class Policy
    {
        public int Id { get; set; }
        // Working day schedule
        public TimeSpan WorkStartTime { get; set; } // e.g., 09:00
        public TimeSpan WorkEndTime { get; set; }   // e.g., 17:00
        public int LateThresholdMinutes { get; set; } // e.g., 15
        public decimal LateDeductionPercent { get; set; } // e.g., 10 (%)
        public int LeaveEarlyThresholdMinutes { get; set; } // e.g., 15
        public decimal LeaveEarlyDeductionPercent { get; set; } // e.g., 10 (%)

        // Overtime monthly limit (hours)
        public int MonthlyOvertimeHoursLimit { get; set; } // e.g., 40

        // Annual leave allocation (days)
        public int AnnualLeaveMaxDays { get; set; } // e.g., 12
    }
}
