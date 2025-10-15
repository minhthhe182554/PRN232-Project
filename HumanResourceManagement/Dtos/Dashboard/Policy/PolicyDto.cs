using System;

namespace HRM_API.Dtos.Dashboard.Policy;

public class PolicyDto
{
    public int Id { get; set; }
    public TimeSpan WorkStartTime { get; set; } 
    public TimeSpan WorkEndTime { get; set; }   
    public int LateEarlyThresholdMinutes { get; set; } 
    public decimal LateEarlyDeductionPercent { get; set; } 
    public int MonthlyOvertimeHoursLimit { get; set; } 
    public int AnnualLeaveMaxDays { get; set; } 
}
