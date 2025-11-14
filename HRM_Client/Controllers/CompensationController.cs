using HRM_Client.Models;
using HRM_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class CompensationController : BaseController
    {
        private readonly SalaryScaleService _salaryScaleService;

        public CompensationController(SalaryScaleService salaryScaleService)
        {
            _salaryScaleService = salaryScaleService;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is Admin
            if (ViewBag.Role != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var salaryScales = await _salaryScaleService.GetAllAsync();

            if (salaryScales == null)
            {
                return RedirectToAction("Index", "Error");
            }

            return View(salaryScales);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateSalaryScaleRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var response = await _salaryScaleService.UpdateAsync(request);

            if (response == null)
            {
                return Json(new { success = false, message = "Failed to update salary scale" });
            }

            return Json(new { 
                success = true, 
                message = "Salary scale updated successfully",
                baseSalary = response.BaseSalary
            });
        }
    }
}

