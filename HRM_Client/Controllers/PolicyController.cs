using HRM_Client.Models;
using HRM_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class PolicyController : BaseController
    {
        private readonly PolicyService _policyService;

        public PolicyController(PolicyService policyService)
        {
            _policyService = policyService;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is Admin
            if (ViewBag.Role != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var policy = await _policyService.GetPolicyAsync();

            if (policy == null)
            {
                return RedirectToAction("Index", "Error");
            }

            return View(policy);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdatePolicyRequest request)
        {
            if (!ModelState.IsValid)
            {
                var policy = await _policyService.GetPolicyAsync();
                return View("Index", policy);
            }

            var result = await _policyService.UpdatePolicyAsync(request);

            if (result == null)
            {
                ModelState.AddModelError("", "Failed to update policy");
                var policy = await _policyService.GetPolicyAsync();
                return View("Index", policy);
            }

            TempData["SuccessMessage"] = "Policy updated successfully";
            return RedirectToAction("Index");
        }
    }
}

