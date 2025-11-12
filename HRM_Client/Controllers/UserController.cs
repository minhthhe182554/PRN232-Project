using HRM_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class UserController : BaseController
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
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
            return View(userList);
        }
    }
}

