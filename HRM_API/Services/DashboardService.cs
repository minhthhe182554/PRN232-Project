using HRM_API.Dtos;
using HRM_API.Models.Enums;
using HRM_API.Repositories;

namespace HRM_API.Services
{
    public class DashboardService
    {
        private readonly RequestRepository _requestRepository;
        private readonly AttendanceRepository _attendanceRepository;

        public DashboardService(RequestRepository requestRepository, AttendanceRepository attendanceRepository)
        {
            _requestRepository = requestRepository;
            _attendanceRepository = attendanceRepository;
        }

        public async Task<DashboardStatsResponse> GetAdminStatsAsync()
        {
            var todayRequests = await _requestRepository.GetTodayRequestsAsync();
            var weekRequests = await _requestRepository.GetWeekRequestsAsync();
            var todayAttendances = await _attendanceRepository.GetTodayAttendancesAsync();
            var weekAttendances = await _attendanceRepository.GetWeekAttendancesAsync();
            var totalEmployees = await _attendanceRepository.GetTotalActiveEmployeesAsync();

            return new DashboardStatsResponse
            {
                RequestStats = new RequestStatsDto
                {
                    Today = new RequestPeriodStats
                    {
                        Total = todayRequests.Count,
                        Pending = todayRequests.Count(r => r.Status == RequestStatus.Pending),
                        Approved = todayRequests.Count(r => r.Status == RequestStatus.Approved),
                        Rejected = todayRequests.Count(r => r.Status == RequestStatus.Rejected)
                    },
                    ThisWeek = new RequestPeriodStats
                    {
                        Total = weekRequests.Count,
                        Pending = weekRequests.Count(r => r.Status == RequestStatus.Pending),
                        Approved = weekRequests.Count(r => r.Status == RequestStatus.Approved),
                        Rejected = weekRequests.Count(r => r.Status == RequestStatus.Rejected)
                    }
                },
                AttendanceStats = new AttendanceStatsDto
                {
                    Today = new AttendancePeriodStats
                    {
                        Total = totalEmployees,
                        Present = todayAttendances.Count(a => a.Status == AttendanceStatus.Present),
                        Absent = todayAttendances.Count(a => a.Status == AttendanceStatus.Absent),
                        Late = todayAttendances.Count(a => a.Status == AttendanceStatus.Late),
                        OnLeave = todayAttendances.Count(a => a.Status == AttendanceStatus.LeaveEarly)
                    },
                    ThisWeek = new AttendancePeriodStats
                    {
                        Total = totalEmployees * 7, // Total possible attendance slots for the week
                        Present = weekAttendances.Count(a => a.Status == AttendanceStatus.Present),
                        Absent = weekAttendances.Count(a => a.Status == AttendanceStatus.Absent),
                        Late = weekAttendances.Count(a => a.Status == AttendanceStatus.Late),
                        OnLeave = weekAttendances.Count(a => a.Status == AttendanceStatus.LeaveEarly)
                    }
                }
            };
        }
    }
}

