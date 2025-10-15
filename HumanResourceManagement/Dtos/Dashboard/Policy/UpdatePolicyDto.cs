using System;

namespace HRM_API.Dtos.Dashboard.Policy;

public class UpdatePolicyDto
{
    public int Id { get; set; }

    // Working day schedule
    public TimeSpan WorkStartTime { get; set; } // e.g., 09:00
    public TimeSpan WorkEndTime { get; set; }   // e.g., 17:00

    // Late/Early threshold and deduction
    public int LateEarlyThresholdMinutes { get; set; } // e.g., 15
    public decimal LateEarlyDeductionPercent { get; set; } // e.g., 10 (%)

    // Overtime monthly limit (hours)
    public int MonthlyOvertimeHoursLimit { get; set; } // e.g., 40

    // Annual leave allocation (days)
    public int AnnualLeaveMaxDays { get; set; } // e.g., 12
}
