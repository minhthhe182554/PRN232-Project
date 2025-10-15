using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HumanResourceManagement.Dtos.Dashboard;

namespace HumanResourceManagement.Controllers.Employee;

[ApiController]
[Route("api/employee")]
[Authorize(Roles = "Employee")] 
public class EmployeeController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetEmployeeDashboard()
    {
        var dashboard = new DashboardDto
        {
            Role = "Employee",
            LoginTime = DateTime.Now
        };

        return Ok(dashboard);
    }
}