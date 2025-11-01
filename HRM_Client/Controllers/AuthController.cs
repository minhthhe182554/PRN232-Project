using HRM_Client.Models;
using HRM_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to dashboard
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                ViewBag.Error = "Invalid username or password";
                return View(request);
            }

            // Save token to cookie
            _authService.SaveToken(response.Token);

            // Save user info to session
            HttpContext.Session.SetString("UserId", response.UserId.ToString());
            HttpContext.Session.SetString("Username", response.Username);
            HttpContext.Session.SetString("FullName", response.FullName);
            HttpContext.Session.SetString("Role", response.Role);
            HttpContext.Session.SetInt32("Level", response.Level);
            if (!string.IsNullOrEmpty(response.DepartmentName))
            {
                HttpContext.Session.SetString("DepartmentName", response.DepartmentName);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            _authService.Logout();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

