namespace HRM_API.Dtos
{
    public class IncomeStatisticsResponse
    {
        public IncomeTodayDto Today { get; set; }
        public IncomeMonthlyDto Monthly { get; set; }
    }

    public class IncomeTodayDto
    {
        public decimal BaseSalary { get; set; }
        public decimal DailySalary { get; set; }
        public string AttendanceStatus { get; set; }
        public decimal DeductionAmount { get; set; }
        public decimal FinalSalary { get; set; }
        public string DeductionReason { get; set; }
    }

    public class IncomeMonthlyDto
    {
        public decimal BaseSalary { get; set; }
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int LateDays { get; set; }
        public int LeaveEarlyDays { get; set; }
        public int AbsentDays { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal FinalSalary { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
    }
}

