using HRM_API.Dtos;
using HRM_API.Models.Enums;
using HRM_API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Services
{
    public class PerformanceEvaluationService
    {
        private readonly UserRepository _userRepository;
        private readonly AttendanceRepository _attendanceRepository;
        private readonly RequestRepository _requestRepository;

        public PerformanceEvaluationService(
            UserRepository userRepository,
            AttendanceRepository attendanceRepository,
            RequestRepository requestRepository)
        {
            _userRepository = userRepository;
            _attendanceRepository = attendanceRepository;
            _requestRepository = requestRepository;
        }

        public async Task<GeneralEvaluationRequest> AggregatePerformanceDataAsync()
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-7);

            // Get all active employees and managers (exclude Admin)
            var users = await _userRepository.GetAllWithDetailsAsync();
            var activeEmployees = users
                .Where(u => u.IsActive && (u.Role == Role.Manager || u.Role == Role.Employee))
                .ToList();

            var employeeDataList = new List<EmployeePerformanceData>();

            foreach (var user in activeEmployees)
            {
                // Get attendance data for last 7 days
                var attendances = await _attendanceRepository.GetAttendanceHistoryByUserIdAsync(
                    user.Id, startDate, endDate);

                // Calculate attendance metrics
                var presentDays = attendances.Count(a => a.Status == AttendanceStatus.Present);
                var lateDays = attendances.Count(a => a.Status == AttendanceStatus.Late);
                var leaveEarlyDays = attendances.Count(a => a.Status == AttendanceStatus.LeaveEarly);
                var absentDays = attendances.Count(a => a.Status == AttendanceStatus.Absent);
                var totalWorkingDays = presentDays + lateDays + leaveEarlyDays + absentDays;

                // Calculate average working hours (only for days with check-out)
                var daysWithCheckout = attendances.Where(a => a.CheckOut.HasValue && a.WorkingHours.HasValue).ToList();
                var averageWorkingHours = daysWithCheckout.Any()
                    ? daysWithCheckout.Average(a => a.WorkingHours!.Value.TotalHours)
                    : 0.0;

                // Calculate attendance rate (total attendance / expected working days)
                // Expected working days in 7 days ≈ 5 days (assuming 5 days/week)
                var expectedWorkingDays = 5;
                var attendanceRate = expectedWorkingDays > 0
                    ? (double)(presentDays + lateDays + leaveEarlyDays) / expectedWorkingDays * 100
                    : 0.0;

                // Get request data for last 7 days
                var allRequests = await _requestRepository.GetRequestsByUserIdAsync(user.Id);
                var recentRequests = allRequests
                    .Where(r => r.StartDate.Date >= startDate && r.StartDate.Date <= endDate)
                    .ToList();

                var totalRequests = recentRequests.Count;
                var approvedRequests = recentRequests.Count(r => r.Status == RequestStatus.Approved);
                var rejectedRequests = recentRequests.Count(r => r.Status == RequestStatus.Rejected);
                var pendingRequests = recentRequests.Count(r => r.Status == RequestStatus.Pending);
                var leaveRequests = recentRequests.Count(r => r.Type == RequestType.Leave);
                var resignationRequests = recentRequests.Count(r => r.Type == RequestType.Resignation);
                var overtimeRequests = recentRequests.Count(r => r.Type == RequestType.Overtime);

                var employeeData = new EmployeePerformanceData
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Role = user.Role.ToString(),
                    Level = user.Level,
                    Department = user.Department?.Name,
                    Attendance = new AttendanceData
                    {
                        TotalWorkingDays = totalWorkingDays,
                        PresentDays = presentDays,
                        LateDays = lateDays,
                        LeaveEarlyDays = leaveEarlyDays,
                        AbsentDays = absentDays,
                        AverageWorkingHours = Math.Round(averageWorkingHours, 2),
                        AttendanceRate = Math.Round(attendanceRate, 2)
                    },
                    Requests = new RequestData
                    {
                        Total = totalRequests,
                        Approved = approvedRequests,
                        Rejected = rejectedRequests,
                        Pending = pendingRequests,
                        LeaveRequests = leaveRequests,
                        ResignationRequests = resignationRequests,
                        OvertimeRequests = overtimeRequests
                    }
                };

                employeeDataList.Add(employeeData);
            }

            return new GeneralEvaluationRequest
            {
                EvaluationPeriod = "7 days",
                TotalEmployees = employeeDataList.Count,
                Employees = employeeDataList
            };
        }

        public async Task<LevelPromotionEvaluationRequest> AggregateLevelPromotionDataAsync(Role role)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-30);

            // Get all active users with the specified role
            var users = await _userRepository.GetAllWithDetailsAsync();
            
            // Filter by role and max level (Manager max = 3, Employee max = 3)
            const int maxLevel = 3;
            var eligibleUsers = users
                .Where(u => u.IsActive && u.Role == role && u.Level < maxLevel)
                .ToList();

            var candidateDataList = new List<EmployeePerformanceData>();

            foreach (var user in eligibleUsers)
            {
                // Get attendance data for last 30 days
                var attendances = await _attendanceRepository.GetAttendanceHistoryByUserIdAsync(
                    user.Id, startDate, endDate);

                // Calculate attendance metrics
                var presentDays = attendances.Count(a => a.Status == AttendanceStatus.Present);
                var lateDays = attendances.Count(a => a.Status == AttendanceStatus.Late);
                var leaveEarlyDays = attendances.Count(a => a.Status == AttendanceStatus.LeaveEarly);
                var absentDays = attendances.Count(a => a.Status == AttendanceStatus.Absent);
                var totalWorkingDays = presentDays + lateDays + leaveEarlyDays + absentDays;

                // Calculate average working hours (only for days with check-out)
                var daysWithCheckout = attendances.Where(a => a.CheckOut.HasValue && a.WorkingHours.HasValue).ToList();
                var averageWorkingHours = daysWithCheckout.Any()
                    ? daysWithCheckout.Average(a => a.WorkingHours!.Value.TotalHours)
                    : 0.0;

                // Calculate attendance rate (total attendance / expected working days)
                // Expected working days in 30 days ≈ 22 days (assuming 5 days/week)
                var expectedWorkingDays = 22;
                var attendanceRate = expectedWorkingDays > 0
                    ? (double)(presentDays + lateDays + leaveEarlyDays) / expectedWorkingDays * 100
                    : 0.0;

                // Get all requests for the user (approval rate should be calculated on all requests, not just 30 days)
                var allRequests = await _requestRepository.GetRequestsByUserIdAsync(user.Id);
                
                // Log for debugging
                Console.WriteLine($"User {user.FullName} (ID: {user.Id}): Total requests = {allRequests.Count}");
                if (allRequests.Any())
                {
                    var statusCounts = allRequests.GroupBy(r => r.Status).Select(g => $"{g.Key}:{g.Count()}");
                    Console.WriteLine($"  Status breakdown: {string.Join(", ", statusCounts)}");
                }
                
                // For promotion evaluation, use all requests to calculate approval rate
                // But also track requests in the evaluation period
                var requestsInPeriod = allRequests
                    .Where(r => r.StartDate.Date >= startDate && r.StartDate.Date <= endDate)
                    .ToList();

                // Calculate approval rate based on ALL requests (not just in period)
                // This gives a better picture of the employee's overall request behavior
                var totalRequests = allRequests.Count;
                var approvedRequests = allRequests.Count(r => r.Status == RequestStatus.Approved);
                var rejectedRequests = allRequests.Count(r => r.Status == RequestStatus.Rejected);
                var pendingRequests = allRequests.Count(r => r.Status == RequestStatus.Pending);
                
                // Log approval rate calculation
                var approvalRate = totalRequests > 0 ? (double)approvedRequests / totalRequests * 100 : 0.0;
                Console.WriteLine($"  Approval rate: {approvedRequests}/{totalRequests} = {approvalRate:F1}%");
                
                // Count by type for the evaluation period
                var leaveRequests = requestsInPeriod.Count(r => r.Type == RequestType.Leave);
                var resignationRequests = requestsInPeriod.Count(r => r.Type == RequestType.Resignation);
                var overtimeRequests = requestsInPeriod.Count(r => r.Type == RequestType.Overtime);

                var candidateData = new EmployeePerformanceData
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Role = user.Role.ToString(),
                    Level = user.Level,
                    Department = user.Department?.Name,
                    Attendance = new AttendanceData
                    {
                        TotalWorkingDays = totalWorkingDays,
                        PresentDays = presentDays,
                        LateDays = lateDays,
                        LeaveEarlyDays = leaveEarlyDays,
                        AbsentDays = absentDays,
                        AverageWorkingHours = Math.Round(averageWorkingHours, 2),
                        AttendanceRate = Math.Round(attendanceRate, 2)
                    },
                    Requests = new RequestData
                    {
                        Total = totalRequests,
                        Approved = approvedRequests,
                        Rejected = rejectedRequests,
                        Pending = pendingRequests,
                        LeaveRequests = leaveRequests,
                        ResignationRequests = resignationRequests,
                        OvertimeRequests = overtimeRequests
                    }
                };

                candidateDataList.Add(candidateData);
            }

            return new LevelPromotionEvaluationRequest
            {
                EvaluationPeriod = "30 days",
                Role = role.ToString(),
                TotalCandidates = candidateDataList.Count,
                Candidates = candidateDataList
            };
        }
    }
}

