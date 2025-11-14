using HRM_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class ManagerController : BaseController
    {
        private readonly ManagerService _managerService;

        public ManagerController(ManagerService managerService)
        {
            _managerService = managerService;
        }

        public async Task<IActionResult> Employees()
        {
            if (ViewBag.Role != "Manager")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var employees = await _managerService.GetDepartmentEmployeesAsync();
            return View(employees);
        }

        public async Task<IActionResult> Attendance()
        {
            if (ViewBag.Role != "Manager")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // Load default: last week
            var history = await _managerService.GetAttendanceHistoryAsync("week");
            ViewBag.AttendanceHistory = history;
            ViewBag.SelectedPeriod = "week";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetHistory([FromBody] string period)
        {
            var history = await _managerService.GetAttendanceHistoryAsync(period);
            
            if (history == null)
                return Json(new { success = false, message = "Failed to load history" });

            return Json(new { success = true, data = history });
        }

        [HttpPost]
        public async Task<IActionResult> SearchByDate([FromBody] string dateStr)
        {
            if (!DateTime.TryParse(dateStr, out var date))
                return Json(new { success = false, message = "Invalid date format" });

            var result = await _managerService.SearchAttendanceByDateAsync(date);
            
            if (result.Record == null)
                return Json(new { success = false, message = result.ErrorMessage ?? "Not found" });

            return Json(new { success = true, data = result.Record });
        }

        public async Task<IActionResult> Requests(string? status = null, string? type = null)
        {
            if (ViewBag.Role != "Manager")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var requests = await _managerService.GetDepartmentRequestsAsync(type);
            
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
            ViewBag.RequestType = type ?? "All";
            ViewBag.Requests = requests;

            return View(requests);
        }

        public async Task<IActionResult> Income()
        {
            if (ViewBag.Role != "Manager")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var statistics = await _managerService.GetIncomeStatisticsAsync();
            return View(statistics);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRequestStatus([FromBody] Models.UpdateRequestStatusRequest request)
        {
            var result = await _managerService.UpdateRequestStatusAsync(request.RequestId, request.Status);
            
            if (!result.Success)
                return Json(new { success = false, message = result.Message });

            return Json(new { success = true, message = result.Message });
        }
    }
}

