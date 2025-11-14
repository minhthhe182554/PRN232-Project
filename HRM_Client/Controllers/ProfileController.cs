using HRM_Client.Models;
using HRM_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly ProfileService _profileService;

        public ProfileController(ProfileService profileService)
        {
            _profileService = profileService;
        }

        public async Task<IActionResult> Index()
        {
            var profile = await _profileService.GetProfileAsync();
            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var success = await _profileService.UpdateProfileAsync(request);

            if (!success)
                return Json(new { success = false, message = "Failed to update profile" });

            // Update session FullName if changed
            HttpContext.Session.SetString("FullName", request.FullName);

            return Json(new { success = true, message = "Profile updated successfully" });
        }
    }
}

