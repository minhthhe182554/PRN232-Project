using HRM_API.Dtos;
using HRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM_API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/departments")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly DepartmentService _departmentService;

        public DepartmentController(DepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _departmentService.GetAllAsync();
            return Ok(departments);
        }

        [HttpPut("{id}/name")]
        public async Task<IActionResult> UpdateName(int id, [FromBody] UpdateDepartmentNameRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            request.Id = id;
            var response = await _departmentService.UpdateNameAsync(request);

            if (response == null)
                return NotFound(new { message = "Department not found" });

            return Ok(response);
        }

        [HttpPut("{id}/manager")]
        public async Task<IActionResult> UpdateManager(int id, [FromBody] UpdateDepartmentManagerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            request.DepartmentId = id;
            var response = await _departmentService.UpdateManagerAsync(request);

            if (response == null)
                return BadRequest(new { message = "Failed to update manager. The new manager must be an active Employee in the same department." });

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _departmentService.DeleteAsync(id);

            if (!success)
                return BadRequest(new { message = "Cannot delete department. It may have employees or does not exist." });

            return Ok(new { message = "Department deleted successfully" });
        }

        [HttpGet("search-employees")]
        public async Task<IActionResult> SearchEmployees([FromQuery] string username, [FromQuery] int? departmentId = null)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length < 2)
            {
                return BadRequest(new { message = "Username must be at least 2 characters" });
            }

            var employees = await _departmentService.SearchEmployeesAsync(username, departmentId);
            return Ok(employees);
        }

        [HttpGet("{id}/employees")]
        public async Task<IActionResult> GetEmployeesByDepartment(int id)
        {
            var employees = await _departmentService.GetEmployeesByDepartmentAsync(id);
            return Ok(employees);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _departmentService.CreateAsync(request);

            if (response == null)
                return BadRequest(new { message = "Failed to create department. The manager must be an active Employee." });

            return Ok(response);
        }
    }
}

