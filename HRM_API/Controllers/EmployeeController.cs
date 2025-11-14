using HRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRM_API.Controllers
{
    [Authorize(Roles = "Employee")]
    [Route("api/employee")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var stats = await _employeeService.GetEmployeeStatsAsync(userId);
            
            if (stats == null)
                return NotFound(new { message = "Employee not found" });

            return Ok(stats);
        }

        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _employeeService.CheckInAsync(userId);
            
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

            var result = await _employeeService.CheckOutAsync(userId);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { 
                message = result.Message, 
                isOnTime = result.IsOnTime 
            });
        }

        [HttpGet("income")]
        public async Task<IActionResult> GetIncomeStatistics()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var statistics = await _employeeService.GetIncomeStatisticsAsync(userId);
            
            if (statistics == null)
                return NotFound(new { message = "Employee or salary scale not found" });

            return Ok(statistics);
        }

        [HttpGet("attendance/history")]
        public async Task<IActionResult> GetAttendanceHistory([FromQuery] string period = "week")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (period != "week" && period != "month")
                return BadRequest(new { message = "Period must be 'week' or 'month'" });

            var history = await _employeeService.GetAttendanceHistoryAsync(userId, period);
            
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

            var attendance = await _employeeService.GetAttendanceByDateAsync(userId, date);
            
            if (attendance == null)
                return NotFound(new { message = "Ngày đó công ty chưa mở cửa" });

            return Ok(attendance);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var requests = await _employeeService.GetMyRequestsAsync(userId);
            
            if (requests == null)
                return NotFound(new { message = "Employee not found" });

            return Ok(requests);
        }

        [HttpPost("requests")]
        public async Task<IActionResult> CreateRequest([FromBody] Dtos.CreateRequestDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _employeeService.CreateRequestAsync(userId, request);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message, request = result.Request });
        }
    }
}

