using HRM_API.Dtos;
using HRM_API.Models;
using HRM_API.Models.Enums;
using HRM_API.Repositories;

namespace HRM_API.Services
{
    public class EmployeeService
    {
        private readonly UserRepository _userRepository;
        private readonly AttendanceRepository _attendanceRepository;
        private readonly PolicyRepository _policyRepository;
        private readonly SalaryScaleRepository _salaryScaleRepository;
        private readonly RequestRepository _requestRepository;

        public EmployeeService(
            UserRepository userRepository,
            AttendanceRepository attendanceRepository,
            PolicyRepository policyRepository,
            SalaryScaleRepository salaryScaleRepository,
            RequestRepository requestRepository)
        {
            _userRepository = userRepository;
            _attendanceRepository = attendanceRepository;
            _policyRepository = policyRepository;
            _salaryScaleRepository = salaryScaleRepository;
            _requestRepository = requestRepository;
        }

        public async Task<EmployeeDashboardStatsResponse?> GetEmployeeStatsAsync(int employeeId)
        {
            var employee = await _userRepository.GetByIdAsync(employeeId);
            if (employee == null)
                return null;

            // Check-in/check-out status
            var todayAttendance = await _attendanceRepository.GetTodayAttendanceByUserIdAsync(employeeId);
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

            return new EmployeeDashboardStatsResponse
            {
                CheckInStatus = checkInStatus
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

        public async Task<IncomeStatisticsResponse?> GetIncomeStatisticsAsync(int employeeId)
        {
            var employee = await _userRepository.GetByIdAsync(employeeId);
            if (employee == null)
                return null;

            var policy = await _policyRepository.GetAll();
            if (policy == null)
                return null;

            // Get base salary from SalaryScale
            var salaryScale = await _salaryScaleRepository.GetSalaryAsync(employee.Role, employee.Level);
            if (salaryScale == null)
                return null;

            var baseSalary = salaryScale.BaseSalary;
            
            // Calculate daily salary (assuming 22 working days per month)
            var workingDaysPerMonth = 22;
            var dailySalary = baseSalary / workingDaysPerMonth;

            // Calculate Today income
            var todayAttendance = await _attendanceRepository.GetTodayAttendanceByUserIdAsync(employeeId);
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
            var monthlyAttendances = await _attendanceRepository.GetMonthlyAttendancesByUserIdAsync(employeeId, now.Year, now.Month);
            
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

        public async Task<AttendanceHistoryResponse?> GetAttendanceHistoryAsync(int userId, string period)
        {
            DateTime startDate;
            DateTime endDate = DateTime.Today;
            string periodName;

            if (period == "week")
            {
                endDate = DateTime.Today;
                startDate = endDate.AddDays(-7);
                periodName = "Last Week";
            }
            else if (period == "month")
            {
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

        public async Task<RequestListResponse?> GetMyRequestsAsync(int userId)
        {
            var requests = await _requestRepository.GetRequestsByUserIdAsync(userId);

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

        public async Task<(bool Success, string Message, RequestResponseDto? Request)> CreateRequestAsync(int userId, CreateRequestDto request)
        {
            var employee = await _userRepository.GetByIdAsync(userId);
            if (employee == null)
                return (false, "Employee not found", null);

            if (employee.DepartmentId == null)
                return (false, "You must be assigned to a department to create requests", null);

            if (request.StartDate > request.EndDate)
                return (false, "Start date must be before or equal to end date", null);

            var newRequest = new Request
            {
                UserId = userId,
                DepartmentId = employee.DepartmentId,
                Type = request.Type,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Content = request.Content,
                Status = RequestStatus.Pending
            };

            var createdRequest = await _requestRepository.CreateRequestAsync(newRequest);

            var requestDto = new RequestResponseDto
            {
                Id = createdRequest.Id,
                UserId = createdRequest.UserId,
                UserFullName = employee.FullName,
                UserLevel = employee.Level,
                Type = createdRequest.Type,
                StartDate = createdRequest.StartDate,
                EndDate = createdRequest.EndDate,
                Content = createdRequest.Content,
                Status = createdRequest.Status
            };

            return (true, "Request created successfully", requestDto);
        }
    }
}

