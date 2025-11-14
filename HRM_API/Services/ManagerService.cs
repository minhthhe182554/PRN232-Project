using HRM_API.Dtos;
using HRM_API.Models;
using HRM_API.Models.Enums;
using HRM_API.Repositories;

namespace HRM_API.Services
{
    public class ManagerService
    {
        private readonly UserRepository _userRepository;
        private readonly AttendanceRepository _attendanceRepository;
        private readonly RequestRepository _requestRepository;
        private readonly PolicyRepository _policyRepository;
        private readonly SalaryScaleRepository _salaryScaleRepository;

        public ManagerService(
            UserRepository userRepository,
            AttendanceRepository attendanceRepository,
            RequestRepository requestRepository,
            PolicyRepository policyRepository,
            SalaryScaleRepository salaryScaleRepository)
        {
            _userRepository = userRepository;
            _attendanceRepository = attendanceRepository;
            _requestRepository = requestRepository;
            _policyRepository = policyRepository;
            _salaryScaleRepository = salaryScaleRepository;
        }

        public async Task<ManagerDashboardStatsResponse?> GetManagerStatsAsync(int managerId)
        {
            // Get manager's department
            var manager = await _userRepository.GetByIdAsync(managerId);
            if (manager == null || manager.DepartmentId == null)
                return null;

            var departmentId = manager.DepartmentId.Value;

            // Check-in/check-out status
            var todayAttendance = await _attendanceRepository.GetTodayAttendanceByUserIdAsync(managerId);
            var policy = await _policyRepository.GetAll();
            
            var checkInStatus = new CheckInStatusDto
            {
                HasCheckedInToday = todayAttendance != null,
                CheckInTime = todayAttendance?.CheckIn,
                HasCheckedOutToday = todayAttendance?.CheckOut != null,
                CheckOutTime = todayAttendance?.CheckOut
            };

            if (todayAttendance != null && policy != null)
            {
                // Check-in status
                var checkInTime = todayAttendance.CheckIn.TimeOfDay;
                var workStartTime = policy.WorkStartTime;
                var lateThreshold = TimeSpan.FromMinutes(policy.LateThresholdMinutes);
                
                if (checkInTime <= workStartTime.Add(lateThreshold))
                {
                    checkInStatus.IsCheckInOnTime = true;
                    checkInStatus.CheckInMessage = "On-time";
                }
                else
                {
                    checkInStatus.IsCheckInOnTime = false;
                    var minutesLate = (int)(checkInTime - workStartTime).TotalMinutes;
                    checkInStatus.CheckInMessage = $"Late by {minutesLate} minutes";
                }

                // Check-out status
                if (todayAttendance.CheckOut.HasValue)
                {
                    var checkOutTime = todayAttendance.CheckOut.Value.TimeOfDay;
                    var workEndTime = policy.WorkEndTime;
                    var leaveEarlyThreshold = TimeSpan.FromMinutes(policy.LeaveEarlyThresholdMinutes);
                    
                    if (checkOutTime >= workEndTime.Subtract(leaveEarlyThreshold))
                    {
                        checkInStatus.IsCheckOutOnTime = true;
                        checkInStatus.CheckOutMessage = "On-time";
                    }
                    else
                    {
                        checkInStatus.IsCheckOutOnTime = false;
                        var minutesEarly = (int)(workEndTime - checkOutTime).TotalMinutes;
                        checkInStatus.CheckOutMessage = $"Left early by {minutesEarly} minutes";
                    }
                }
            }

            // Team stats
            var teamMembers = await _userRepository.GetEmployeesByDepartmentIdAsync(departmentId);
            var teamStats = new TeamStatsDto
            {
                TotalMembers = teamMembers.Count,
                Level1Count = teamMembers.Count(u => u.Level == 1),
                Level2Count = teamMembers.Count(u => u.Level == 2),
                Level3Count = teamMembers.Count(u => u.Level == 3)
            };

            // Request stats
            var todayRequests = await _requestRepository.GetTodayRequestsByDepartmentAsync(departmentId);
            var weekRequests = await _requestRepository.GetWeekRequestsByDepartmentAsync(departmentId);

            var requestStats = new RequestStatsDto
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
            };

            // Attendance stats
            var todayAttendances = await _attendanceRepository.GetTodayAttendancesByDepartmentAsync(departmentId);
            var weekAttendances = await _attendanceRepository.GetWeekAttendancesByDepartmentAsync(departmentId);
            var totalMembers = teamMembers.Count;

            var attendanceStats = new AttendanceStatsDto
            {
                Today = new AttendancePeriodStats
                {
                    Total = totalMembers,
                    Present = todayAttendances.Count(a => a.Status == AttendanceStatus.Present),
                    Absent = todayAttendances.Count(a => a.Status == AttendanceStatus.Absent),
                    Late = todayAttendances.Count(a => a.Status == AttendanceStatus.Late),
                    OnLeave = todayAttendances.Count(a => a.Status == AttendanceStatus.LeaveEarly)
                },
                ThisWeek = new AttendancePeriodStats
                {
                    Total = totalMembers * 7,
                    Present = weekAttendances.Count(a => a.Status == AttendanceStatus.Present),
                    Absent = weekAttendances.Count(a => a.Status == AttendanceStatus.Absent),
                    Late = weekAttendances.Count(a => a.Status == AttendanceStatus.Late),
                    OnLeave = weekAttendances.Count(a => a.Status == AttendanceStatus.LeaveEarly)
                }
            };

            // Top absentees (this week)
            var topAbsentees = weekAttendances
                .Where(a => a.Status == AttendanceStatus.Absent)
                .GroupBy(a => a.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    AbsentCount = g.Count(),
                    User = g.First().User
                })
                .OrderByDescending(x => x.AbsentCount)
                .Take(5)
                .Select(x => new TopAbsenteeDto
                {
                    UserId = x.UserId,
                    FullName = x.User.FullName,
                    Level = x.User.Level,
                    AbsentCount = x.AbsentCount
                })
                .ToList();

            return new ManagerDashboardStatsResponse
            {
                CheckInStatus = checkInStatus,
                TeamStats = teamStats,
                RequestStats = requestStats,
                AttendanceStats = attendanceStats,
                TopAbsentees = topAbsentees
            };
        }

        public async Task<(bool Success, string Message, bool IsOnTime)> CheckInAsync(int userId)
        {
            // Check if already checked in today
            var existingAttendance = await _attendanceRepository.GetTodayAttendanceByUserIdAsync(userId);
            if (existingAttendance != null)
                return (false, "Already checked in today", false);

            var policy = await _policyRepository.GetAll();
            if (policy == null)
                return (false, "Policy not found", false);

            var now = DateTime.Now;
            var checkInTime = now.TimeOfDay;
            var workStartTime = policy.WorkStartTime;
            var lateThreshold = TimeSpan.FromMinutes(policy.LateThresholdMinutes);
            
            AttendanceStatus status;
            bool isOnTime;
            string message;

            if (checkInTime <= workStartTime.Add(lateThreshold))
            {
                status = AttendanceStatus.Present;
                isOnTime = true;
                message = "Checked in on-time";
            }
            else
            {
                status = AttendanceStatus.Late;
                isOnTime = false;
                var minutesLate = (int)(checkInTime - workStartTime).TotalMinutes;
                message = $"Checked in late by {minutesLate} minutes";
            }

            var attendance = new Attendance
            {
                UserId = userId,
                CheckIn = now,
                Status = status
            };

            await _attendanceRepository.CreateAttendanceAsync(attendance);
            return (true, message, isOnTime);
        }

        public async Task<(bool Success, string Message, bool IsOnTime)> CheckOutAsync(int userId)
        {
            // Check if already checked in today
            var existingAttendance = await _attendanceRepository.GetTodayAttendanceByUserIdAsync(userId);
            if (existingAttendance == null)
                return (false, "Please check in first", false);

            // Check if already checked out today
            if (existingAttendance.CheckOut != null)
                return (false, "Already checked out today", false);

            var policy = await _policyRepository.GetAll();
            if (policy == null)
                return (false, "Policy not found", false);

            var now = DateTime.Now;
            var checkOutTime = now.TimeOfDay;
            var workEndTime = policy.WorkEndTime;
            var leaveEarlyThreshold = TimeSpan.FromMinutes(policy.LeaveEarlyThresholdMinutes);
            
            bool isOnTime;
            string message;

            if (checkOutTime >= workEndTime.Subtract(leaveEarlyThreshold))
            {
                isOnTime = true;
                message = "Checked out on-time";
            }
            else
            {
                isOnTime = false;
                var minutesEarly = (int)(workEndTime - checkOutTime).TotalMinutes;
                message = $"Checked out early by {minutesEarly} minutes";
                
                // Update status to LeaveEarly if not already Late
                if (existingAttendance.Status == AttendanceStatus.Present)
                {
                    existingAttendance.Status = AttendanceStatus.LeaveEarly;
                }
            }

            existingAttendance.CheckOut = now;
            await _attendanceRepository.UpdateAttendanceAsync(existingAttendance);
            return (true, message, isOnTime);
        }

        public async Task<UserList?> GetDepartmentEmployeesAsync(int managerId)
        {
            var manager = await _userRepository.GetByIdAsync(managerId);
            if (manager == null || manager.DepartmentId == null)
                return null;

            var employees = await _userRepository.GetEmployeesByDepartmentIdAsync(manager.DepartmentId.Value);
            var todayAttendances = await _attendanceRepository.GetTodayAttendancesByDepartmentAsync(manager.DepartmentId.Value);

            var userDtos = employees.Select(u =>
            {
                var todayAttendance = todayAttendances.FirstOrDefault(a => a.UserId == u.Id);
                return new UserListDto
                {
                    Id = u.Id,
                    Role = u.Role,
                    Level = u.Level,
                    FullName = u.FullName,
                    IsActive = u.IsActive,
                    DepartmentName = manager.Department?.Name,
                    TodayAttendanceStatus = todayAttendance?.Status
                };
            }).ToList();

            return new UserList
            {
                ActiveUsers = userDtos.Where(u => u.IsActive).ToList(),
                InactiveUsers = userDtos.Where(u => !u.IsActive).ToList(),
                Count = userDtos.Count
            };
        }

        public async Task<AttendanceHistoryResponse?> GetAttendanceHistoryAsync(int userId, string period)
        {
            DateTime startDate;
            DateTime endDate = DateTime.Today;
            string periodName;

            if (period == "week")
            {
                // Last week (7 days ago to today)
                endDate = DateTime.Today;
                startDate = endDate.AddDays(-7);
                periodName = "Last Week";
            }
            else if (period == "month")
            {
                // Last month (30 days ago to today)
                endDate = DateTime.Today;
                startDate = endDate.AddDays(-30);
                periodName = "Last Month";
            }
            else
            {
                return null;
            }

            var attendances = await _attendanceRepository.GetAttendanceHistoryByUserIdAsync(userId, startDate, endDate);

            var records = attendances.Select(a => new AttendanceRecordDto
            {
                Id = a.Id,
                Date = a.CheckIn.Date,
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                Status = a.Status.ToString(),
                WorkingHours = a.WorkingHours
            }).ToList();

            return new AttendanceHistoryResponse
            {
                Records = records,
                Period = periodName
            };
        }

        public async Task<AttendanceRecordDto?> GetAttendanceByDateAsync(int userId, DateTime date)
        {
            var attendance = await _attendanceRepository.GetAttendanceByUserIdAndDateAsync(userId, date);
            
            if (attendance == null)
                return null;

            return new AttendanceRecordDto
            {
                Id = attendance.Id,
                Date = attendance.CheckIn.Date,
                CheckIn = attendance.CheckIn,
                CheckOut = attendance.CheckOut,
                Status = attendance.Status.ToString(),
                WorkingHours = attendance.WorkingHours
            };
        }

        public async Task<RequestListResponse?> GetDepartmentRequestsAsync(int managerId, RequestType? type = null)
        {
            var manager = await _userRepository.GetByIdAsync(managerId);
            if (manager == null || manager.DepartmentId == null)
                return null;

            var departmentId = manager.DepartmentId.Value;
            var requests = await _requestRepository.GetRequestsByDepartmentAndTypeAsync(departmentId, type);

            var requestDtos = requests.Select(r => new RequestResponseDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserFullName = r.User.FullName,
                UserLevel = r.User.Level,
                Type = r.Type,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Content = r.Content,
                Status = r.Status,
                CreatedAt = null,
                ProcessedAt = null
            }).ToList();

            return new RequestListResponse
            {
                Requests = requestDtos,
                Count = requestDtos.Count
            };
        }

        public async Task<(bool Success, string Message)> UpdateRequestStatusAsync(int managerId, int requestId, RequestStatus status)
        {
            // Verify manager has access to this request
            var manager = await _userRepository.GetByIdAsync(managerId);
            if (manager == null || manager.DepartmentId == null)
                return (false, "Manager or department not found");

            var request = await _requestRepository.GetByIdAsync(requestId);
            if (request == null)
                return (false, "Request not found");

            // Verify request belongs to manager's department
            if (request.User.DepartmentId != manager.DepartmentId.Value)
                return (false, "You don't have permission to process this request");

            // Verify request is pending
            if (request.Status != RequestStatus.Pending)
                return (false, "Request has already been processed");

            var success = await _requestRepository.UpdateRequestStatusAsync(requestId, status);
            if (!success)
                return (false, "Failed to update request status");

            return (true, status == RequestStatus.Approved ? "Request approved successfully" : "Request rejected successfully");
        }

        public async Task<IncomeStatisticsResponse?> GetIncomeStatisticsAsync(int managerId)
        {
            var manager = await _userRepository.GetByIdAsync(managerId);
            if (manager == null)
                return null;

            var policy = await _policyRepository.GetAll();
            if (policy == null)
                return null;

            // Get base salary from SalaryScale
            var salaryScale = await _salaryScaleRepository.GetSalaryAsync(manager.Role, manager.Level);
            if (salaryScale == null)
                return null;

            var baseSalary = salaryScale.BaseSalary;
            
            // Calculate daily salary (assuming 22 working days per month)
            var workingDaysPerMonth = 22;
            var dailySalary = baseSalary / workingDaysPerMonth;

            // Calculate Today income
            var todayAttendance = await _attendanceRepository.GetTodayAttendanceByUserIdAsync(managerId);
            var todayIncome = new IncomeTodayDto
            {
                BaseSalary = baseSalary,
                DailySalary = dailySalary,
                AttendanceStatus = todayAttendance?.Status.ToString() ?? "Absent",
                DeductionAmount = 0,
                FinalSalary = dailySalary,
                DeductionReason = ""
            };

            if (todayAttendance == null)
            {
                // Absent - deduct 100% of daily salary
                todayIncome.DeductionAmount = dailySalary;
                todayIncome.FinalSalary = 0;
                todayIncome.DeductionReason = "Absent (no check-in)";
            }
            else if (todayAttendance.Status == AttendanceStatus.Late)
            {
                // Late - deduct percentage from Policy
                todayIncome.DeductionAmount = dailySalary * (policy.LateDeductionPercent / 100);
                todayIncome.FinalSalary = dailySalary - todayIncome.DeductionAmount;
                todayIncome.DeductionReason = $"Late ({policy.LateDeductionPercent}% deduction)";
            }
            else if (todayAttendance.Status == AttendanceStatus.LeaveEarly)
            {
                // LeaveEarly - deduct percentage from Policy
                todayIncome.DeductionAmount = dailySalary * (policy.LeaveEarlyDeductionPercent / 100);
                todayIncome.FinalSalary = dailySalary - todayIncome.DeductionAmount;
                todayIncome.DeductionReason = $"Leave Early ({policy.LeaveEarlyDeductionPercent}% deduction)";
            }
            else if (todayAttendance.Status == AttendanceStatus.Absent)
            {
                // Absent - deduct 100% of daily salary
                todayIncome.DeductionAmount = dailySalary;
                todayIncome.FinalSalary = 0;
                todayIncome.DeductionReason = "Absent (100% deduction)";
            }
            else
            {
                // Present - no deduction
                todayIncome.DeductionReason = "Present (no deduction)";
            }

            // Calculate Monthly income
            var now = DateTime.Now;
            var monthlyAttendances = await _attendanceRepository.GetMonthlyAttendancesByUserIdAsync(managerId, now.Year, now.Month);
            
            var presentDays = monthlyAttendances.Count(a => a.Status == AttendanceStatus.Present);
            var lateDays = monthlyAttendances.Count(a => a.Status == AttendanceStatus.Late);
            var leaveEarlyDays = monthlyAttendances.Count(a => a.Status == AttendanceStatus.LeaveEarly);
            var absentDays = monthlyAttendances.Count(a => a.Status == AttendanceStatus.Absent);
            
            // Calculate total deduction for the month
            var lateDeduction = lateDays * dailySalary * (policy.LateDeductionPercent / 100);
            var leaveEarlyDeduction = leaveEarlyDays * dailySalary * (policy.LeaveEarlyDeductionPercent / 100);
            var absentDeduction = absentDays * dailySalary; // 100% deduction for absent
            var totalDeduction = lateDeduction + leaveEarlyDeduction + absentDeduction;
            
            // Total working days = days with attendance records
            var totalWorkingDays = monthlyAttendances.Count;
            
            var monthlyIncome = new IncomeMonthlyDto
            {
                BaseSalary = baseSalary,
                TotalWorkingDays = totalWorkingDays,
                PresentDays = presentDays,
                LateDays = lateDays,
                LeaveEarlyDays = leaveEarlyDays,
                AbsentDays = absentDays,
                TotalDeduction = totalDeduction,
                FinalSalary = baseSalary - totalDeduction,
                CurrentMonth = now.Month,
                CurrentYear = now.Year
            };

            return new IncomeStatisticsResponse
            {
                Today = todayIncome,
                Monthly = monthlyIncome
            };
        }
    }
}

