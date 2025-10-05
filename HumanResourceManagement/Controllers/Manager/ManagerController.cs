using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HumanResourceManagement.Dtos.Dashboard;

namespace HumanResourceManagement.Controllers.Manager;

[ApiController]
[Route("api/manager")]
[Authorize(Roles = "Manager")]
public class ManagerController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetManagerDashboard()
    {
        var dashboard = new DashboardDto
        {
            Role = "Manager",
            LoginTime = DateTime.Now,
            Message = "Welcome to Manager Dashboard"
        };

        return Ok(dashboard);
    }
}