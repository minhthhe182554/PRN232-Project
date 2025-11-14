using HRM_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class EmployeeController : BaseController
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Income()
        {
            if (ViewBag.Role != "Employee")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var statistics = await _employeeService.GetIncomeStatisticsAsync();
            return View(statistics);
        }

        public async Task<IActionResult> Attendance()
        {
            if (ViewBag.Role != "Employee")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // Load default: last week
            var history = await _employeeService.GetAttendanceHistoryAsync("week");
            ViewBag.AttendanceHistory = history;
            ViewBag.SelectedPeriod = "week";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetHistory([FromBody] string period)
        {
            var history = await _employeeService.GetAttendanceHistoryAsync(period);
            
            if (history == null)
                return Json(new { success = false, message = "Failed to load history" });

            return Json(new { success = true, data = history });
        }

        [HttpPost]
        public async Task<IActionResult> SearchByDate([FromBody] string dateStr)
        {
            if (!DateTime.TryParse(dateStr, out var date))
                return Json(new { success = false, message = "Invalid date format" });

            var result = await _employeeService.SearchAttendanceByDateAsync(date);
            
            if (result.Record == null)
                return Json(new { success = false, message = result.ErrorMessage ?? "Not found" });

            return Json(new { success = true, data = result.Record });
        }

        public async Task<IActionResult> Requests(string? status = null)
        {
            if (ViewBag.Role != "Employee")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var requests = await _employeeService.GetMyRequestsAsync();
            
            // Filter by status if provided
            if (!string.IsNullOrEmpty(status) && requests != null)
            {
                if (status == "Pending")
                {
                    requests.Requests = requests.Requests.Where(r => r.Status == "Pending").ToList();
                    requests.Count = requests.Requests.Count;
                }
                else if (status == "Processed")
                {
                    requests.Requests = requests.Requests.Where(r => r.Status == "Approved" || r.Status == "Rejected").ToList();
                    requests.Count = requests.Requests.Count;
                }
            }
            
            ViewBag.RequestStatus = status ?? "All";
            ViewBag.Requests = requests;

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] Models.CreateRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var result = await _employeeService.CreateRequestAsync(request);
            
            if (!result.Success)
                return Json(new { success = false, message = result.Message });

            return Json(new { success = true, message = result.Message });
        }
    }
}

