using HRM_Client.Models;
using HRM_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class UserController : BaseController
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;
        private readonly DepartmentService _departmentService;

        public UserController(UserService userService, AuthService authService, DepartmentService departmentService)
        {
            _userService = userService;
            _authService = authService;
            _departmentService = departmentService;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var userList = await _userService.GetAllUsersAsync();
            var departments = await _departmentService.GetAllAsync();
            
            ViewBag.Departments = departments;
            
            return View(userList);
        }

        [HttpPost]
        public async Task<IActionResult> Ban([FromBody] BanUserRequest request)
        {
            var response = await _userService.BanUserAsync(request);

            if (response == null)
            {
                return Json(new { success = false, message = "Failed to update user status" });
            }

            return Json(new { success = true, message = response.Message, isActive = response.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var response = await _userService.ResetPasswordAsync(request);

            if (response == null)
            {
                return Json(new { success = false, message = "Failed to reset password" });
            }

            return Json(new { 
                success = true, 
                message = response.Message, 
                username = response.Username,
                newPassword = response.NewPassword 
            });
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var response = await _authService.RegisterAsync(request);

            if (response == null)
            {
                return Json(new { success = false, message = "Failed to create user" });
            }

            return Json(new
            {
                success = true,
                message = response.Message,
                userId = response.UserId,
                username = response.Username,
                password = response.Password,
                fullName = response.FullName,
                role = response.Role
            });
        }
    }
}

