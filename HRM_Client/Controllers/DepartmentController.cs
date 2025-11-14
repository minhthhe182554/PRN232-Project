using HRM_Client.Models;
using HRM_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class DepartmentController : BaseController
    {
        private readonly DepartmentService _departmentService;
        private readonly UserService _userService;

        public DepartmentController(DepartmentService departmentService, UserService userService)
        {
            _departmentService = departmentService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            if (ViewBag.Role != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var departments = await _departmentService.GetAllAsync();
            return View(departments);
        }

        public async Task<IActionResult> ChangeManager(int id)
        {
            if (ViewBag.Role != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var departments = await _departmentService.GetAllAsync();
            var department = departments?.FirstOrDefault(d => d.Id == id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        [HttpGet]
        public async Task<IActionResult> SearchEmployees([FromQuery] string username, [FromQuery] int? departmentId = null)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length < 2)
            {
                return Json(new { success = false, message = "Username must be at least 2 characters", employees = new List<EmployeeSearchResult>() });
            }

            var employees = await _departmentService.SearchEmployeesAsync(username, departmentId);

            if (employees == null)
            {
                return Json(new { success = false, message = "Failed to search employees", employees = new List<EmployeeSearchResult>() });
            }

            return Json(new { success = true, employees });
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartmentEmployees([FromQuery] int departmentId)
        {
            var employees = await _departmentService.GetEmployeesByDepartmentAsync(departmentId);

            if (employees == null)
            {
                return Json(new { success = false, message = "Failed to load employees", employees = new List<EmployeeSearchResult>() });
            }

            return Json(new { success = true, employees });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateName([FromBody] UpdateDepartmentNameRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var response = await _departmentService.UpdateNameAsync(request);

            if (response == null)
                return Json(new { success = false, message = "Failed to update department name" });

            return Json(new { success = true, message = "Department name updated successfully", data = response });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateManager([FromBody] UpdateDepartmentManagerRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var response = await _departmentService.UpdateManagerAsync(request);

            if (response == null)
                return Json(new { success = false, message = "Failed to update manager. The new manager must be an active Employee in the same department." });

            return Json(new { success = true, message = "Manager updated successfully. Old manager demoted to Employee Level 3, new manager promoted to Manager Level 1.", data = response });
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            var success = await _departmentService.DeleteAsync(id);

            if (!success)
                return Json(new { success = false, message = "Cannot delete department. It may have employees or does not exist." });

            return Json(new { success = true, message = "Department deleted successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var response = await _departmentService.CreateAsync(request);

            if (response == null)
                return Json(new { success = false, message = "Failed to create department. The manager must be an active Employee." });

            return Json(new { success = true, message = "Department created successfully", data = response });
        }
    }
}

