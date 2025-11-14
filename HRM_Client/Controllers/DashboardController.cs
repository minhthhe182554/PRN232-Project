using HRM_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly UserService _userService;
        private readonly DepartmentService _departmentService;
        private readonly DashboardService _dashboardService;
        private readonly ManagerService _managerService;
        private readonly EmployeeService _employeeService;
        private readonly PerformanceEvaluationService _performanceEvaluationService;

        public DashboardController(
            UserService userService, 
            DepartmentService departmentService, 
            DashboardService dashboardService, 
            ManagerService managerService, 
            EmployeeService employeeService,
            PerformanceEvaluationService performanceEvaluationService)
        {
            _userService = userService;
            _departmentService = departmentService;
            _dashboardService = dashboardService;
            _managerService = managerService;
            _employeeService = employeeService;
            _performanceEvaluationService = performanceEvaluationService;
        }

        public async Task<IActionResult> Index()
        {
            if (ViewBag.Role == "Admin")
            {
                var userList = await _userService.GetAllUsersAsync();
                var departments = await _departmentService.GetAllAsync();
                var stats = await _dashboardService.GetAdminStatsAsync();

                ViewBag.TotalEmployees = userList?.Count ?? 0;
                ViewBag.TotalDepartments = departments?.Count ?? 0;
                ViewBag.DashboardStats = stats;
            }
            else if (ViewBag.Role == "Manager")
            {
                var stats = await _managerService.GetDashboardStatsAsync();
                ViewBag.ManagerStats = stats;
            }
            else if (ViewBag.Role == "Employee")
            {
                var stats = await _employeeService.GetDashboardStatsAsync();
                ViewBag.EmployeeStats = stats;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn()
        {
            var role = ViewBag.Role as string;
            var result = role == "Manager" 
                ? await _managerService.CheckInAsync()
                : await _employeeService.CheckInAsync();
            
            return Json(new { 
                success = result.Success, 
                message = result.Message,
                isOnTime = result.IsOnTime
            });
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut()
        {
            var role = ViewBag.Role as string;
            var result = role == "Manager" 
                ? await _managerService.CheckOutAsync()
                : await _employeeService.CheckOutAsync();
            
            return Json(new { 
                success = result.Success, 
                message = result.Message,
                isOnTime = result.IsOnTime
            });
        }

        [HttpPost]
        public async Task<IActionResult> GeneralEvaluation()
        {
            if (ViewBag.Role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var evaluationText = await _performanceEvaluationService.GetGeneralEvaluationAsync();

                if (string.IsNullOrEmpty(evaluationText))
                {
                    return Json(new { success = false, message = "Failed to generate evaluation" });
                }

                return Json(new { success = true, evaluation = evaluationText });
            }
            catch (TimeoutException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LevelPromotionEvaluation([FromBody] string role)
        {
            if (ViewBag.Role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var evaluationResult = await _performanceEvaluationService.GetLevelPromotionEvaluationAsync(role);

                if (evaluationResult == null)
                {
                    return Json(new { success = false, message = "Failed to generate evaluation" });
                }

                return Json(new { success = true, data = evaluationResult });
            }
            catch (TimeoutException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PromoteLevel([FromBody] int userId)
        {
            if (ViewBag.Role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var result = await _performanceEvaluationService.PromoteLevelAsync(userId);

                if (result == null)
                {
                    return Json(new { success = false, message = "Failed to promote level" });
                }

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}

