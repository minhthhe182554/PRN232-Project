using System.ComponentModel.DataAnnotations;

namespace HRM_Client.Models
{
    public class UpdatePolicyRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Work start time is required")]
        [Display(Name = "Work Start Time")]
        public TimeSpan WorkStartTime { get; set; }

        [Required(ErrorMessage = "Work end time is required")]
        [Display(Name = "Work End Time")]
        public TimeSpan WorkEndTime { get; set; }

        [Required(ErrorMessage = "Late threshold is required")]
        [Range(0, 60, ErrorMessage = "Late threshold must be between 0 and 60 minutes")]
        [Display(Name = "Late Threshold (minutes)")]
        public int LateThresholdMinutes { get; set; }

        [Required(ErrorMessage = "Late deduction percent is required")]
        [Range(0, 60, ErrorMessage = "Deduction percent must be between 0 and 60")]
        [Display(Name = "Late Deduction (%)")]
        public decimal LateDeductionPercent { get; set; }

        [Required(ErrorMessage = "Leave early threshold is required")]
        [Range(0, 60, ErrorMessage = "Leave early threshold must be between 0 and 60 minutes")]
        [Display(Name = "Leave Early Threshold (minutes)")]
        public int LeaveEarlyThresholdMinutes { get; set; }

        [Required(ErrorMessage = "Leave early deduction percent is required")]
        [Range(0, 100, ErrorMessage = "Deduction percent must be between 0 and 100")]
        [Display(Name = "Leave Early Deduction (%)")]
        public decimal LeaveEarlyDeductionPercent { get; set; }

        [Required(ErrorMessage = "Monthly overtime limit is required")]
        [Range(0, 200, ErrorMessage = "Overtime limit must be between 0 and 200 hours")]
        [Display(Name = "Monthly Overtime Limit (hours)")]
        public int MonthlyOvertimeHoursLimit { get; set; }

        [Required(ErrorMessage = "Annual leave days is required")]
        [Range(0, 30, ErrorMessage = "Annual leave must be between 0 and 30 days")]
        [Display(Name = "Annual Leave Days")]
        public int AnnualLeaveMaxDays { get; set; }
    }
}
