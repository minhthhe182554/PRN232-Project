using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HumanResourceManagement.Dtos.Dashboard;

namespace HumanResourceManagement.Controllers.Admin;

[Route("api/admin")]
[Authorize(Roles = "Admin")]
[ApiController]
public class AdminController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetAdminDashboard()
    {
        var dashboard = new DashboardDto
        {
            Role = "Admin",
            LoginTime = DateTime.Now,
            Message = "Welcome to Admin Dashboard"
        };

        return Ok(dashboard);
    }
}