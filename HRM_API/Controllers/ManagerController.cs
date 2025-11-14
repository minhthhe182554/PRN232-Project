using HRM_API.Dtos;
using HRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRM_API.Controllers
{
    [Authorize(Roles = "Manager")]
    [Route("api/manager")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly ManagerService _managerService;

        public ManagerController(ManagerService managerService)
        {
            _managerService = managerService;
        }

        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var stats = await _managerService.GetManagerStatsAsync(userId);
            
            if (stats == null)
                return NotFound(new { message = "Manager or department not found" });

            return Ok(stats);
        }

        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _managerService.CheckInAsync(userId);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { 
                message = result.Message, 
                isOnTime = result.IsOnTime 
            });
        }

        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _managerService.CheckOutAsync(userId);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { 
                message = result.Message, 
                isOnTime = result.IsOnTime 
            });
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetDepartmentEmployees()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var employees = await _managerService.GetDepartmentEmployeesAsync(userId);
            
            if (employees == null)
                return NotFound(new { message = "Manager or department not found" });

            return Ok(employees);
        }

        [HttpGet("attendance/history")]
        public async Task<IActionResult> GetAttendanceHistory([FromQuery] string period = "week")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (period != "week" && period != "month")
                return BadRequest(new { message = "Period must be 'week' or 'month'" });

            var history = await _managerService.GetAttendanceHistoryAsync(userId, period);
            
            if (history == null)
                return NotFound(new { message = "Invalid period" });

            return Ok(history);
        }

        [HttpGet("attendance/search")]
        public async Task<IActionResult> SearchAttendanceByDate([FromQuery] DateTime date)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var attendance = await _managerService.GetAttendanceByDateAsync(userId, date);
            
            if (attendance == null)
                return NotFound(new { message = "Ngày đó công ty chưa mở cửa" });

            return Ok(attendance);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetDepartmentRequests([FromQuery] string? type = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            Models.Enums.RequestType? requestType = null;
            if (!string.IsNullOrEmpty(type) && Enum.TryParse<Models.Enums.RequestType>(type, out var parsedType))
            {
                requestType = parsedType;
            }

            var requests = await _managerService.GetDepartmentRequestsAsync(userId, requestType);
            
            if (requests == null)
                return NotFound(new { message = "Manager or department not found" });

            return Ok(requests);
        }

        [HttpPut("requests/{requestId}/status")]
        public async Task<IActionResult> UpdateRequestStatus(int requestId, [FromBody] Dtos.UpdateRequestStatusRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (request.RequestId != requestId)
                return BadRequest(new { message = "Request ID mismatch" });

            var result = await _managerService.UpdateRequestStatusAsync(userId, requestId, request.Status);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpGet("income")]
        public async Task<IActionResult> GetIncomeStatistics()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var statistics = await _managerService.GetIncomeStatisticsAsync(userId);
            
            if (statistics == null)
                return NotFound(new { message = "Manager or salary scale not found" });

            return Ok(statistics);
        }
    }
}

