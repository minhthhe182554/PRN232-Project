using HRM_API.Dtos;
using HRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM_API.Controllers
{
    [ApiController]
    [Route("api/")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly PerformanceEvaluationService _performanceEvaluationService;
        private readonly GeminiService _geminiService;

        public AdminController(
            UserService userService,
            PerformanceEvaluationService performanceEvaluationService,
            GeminiService geminiService)
        {
            _userService = userService;
            _performanceEvaluationService = performanceEvaluationService;
            _geminiService = geminiService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<UserList>> GetAllUsers()
        {
            var userList = await _userService.GetAllUsersAsync();
            return Ok(userList);
        }

        [HttpPatch("users/ban")]
        public async Task<IActionResult> BanUser([FromBody] BanUserRequest request)
        {
            var response = await _userService.BanUserAsync(request);

            if (response == null)
                return NotFound(new { message = "User not found" });

            return Ok(response);
        }

        [HttpPost("users/reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var response = await _userService.ResetPasswordAsync(request);

            if (response == null)
                return NotFound(new { message = "User not found" });

            return Ok(response);
        }

        [HttpPost("admin/performance/general-evaluation")]
        [Microsoft.AspNetCore.Http.Timeouts.RequestTimeout("LongRunning")]
        public async Task<IActionResult> GeneralEvaluation(CancellationToken cancellationToken)
        {
            try
            {
                // Aggregate performance data for last 7 days
                var performanceData = await _performanceEvaluationService.AggregatePerformanceDataAsync();

                // Check if there's enough data
                if (performanceData.TotalEmployees == 0)
                {
                    return BadRequest(new { message = "No employee data available for evaluation" });
                }

                // Evaluate using Gemini
                var evaluationText = await _geminiService.EvaluatePerformanceAsync(performanceData);

                return Ok(new { evaluation = evaluationText });
            }
            catch (InvalidOperationException ex)
            {
                var errorMessage = ex.Message;
                if (errorMessage.Contains("timed out"))
                {
                    return StatusCode(408, new { message = errorMessage });
                }
                return StatusCode(500, new { message = errorMessage });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, new { message = "Request timed out. The evaluation is taking too long. Please try again or wait a moment." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating evaluation", error = ex.Message });
            }
        }

        [HttpPost("admin/performance/level-promotion-evaluation")]
        [Microsoft.AspNetCore.Http.Timeouts.RequestTimeout("LongRunning")]
        public async Task<IActionResult> LevelPromotionEvaluation([FromBody] string role, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(role) || (role != "Manager" && role != "Employee"))
                {
                    return BadRequest(new { message = "Invalid role. Must be 'Manager' or 'Employee'" });
                }

                var roleEnum = role == "Manager" ? Models.Enums.Role.Manager : Models.Enums.Role.Employee;

                // Aggregate performance data for last 30 days (only candidates below max level)
                var performanceData = await _performanceEvaluationService.AggregateLevelPromotionDataAsync(roleEnum);

                // Check if there are any candidates
                if (performanceData.TotalCandidates == 0)
                {
                    return Ok(new LevelPromotionEvaluationResponse
                    {
                        HasRecommendation = false,
                        RecommendedUserId = null,
                        RecommendedUserName = null,
                        CurrentLevel = null,
                        RecommendedLevel = null,
                        Reason = null
                    });
                }

                // Evaluate using Gemini
                var evaluationResult = await _geminiService.EvaluateLevelPromotionAsync(performanceData);

                return Ok(evaluationResult);
            }
            catch (InvalidOperationException ex)
            {
                var errorMessage = ex.Message;
                if (errorMessage.Contains("timed out"))
                {
                    return StatusCode(408, new { message = errorMessage });
                }
                if (errorMessage.Contains("rate limit"))
                {
                    return StatusCode(429, new { message = errorMessage });
                }
                // Log for debugging
                Console.WriteLine($"Level promotion evaluation error: {ex.Message}");
                return StatusCode(500, new { message = errorMessage });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, new { message = "Request timed out. The evaluation is taking too long. Please try again or wait a moment." });
            }
            catch (Exception ex)
            {
                // Log for debugging
                Console.WriteLine($"Level promotion evaluation exception: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred while generating evaluation", error = ex.Message });
            }
        }

        [HttpPost("admin/performance/promote-level")]
        public async Task<IActionResult> PromoteLevel([FromBody] PromoteLevelRequest request)
        {
            try
            {
                var response = await _userService.PromoteLevelAsync(request.UserId);

                if (response == null)
                {
                    return BadRequest(new { message = "Failed to promote level. User not found or already at max level." });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while promoting level", error = ex.Message });
            }
        }
    }
}