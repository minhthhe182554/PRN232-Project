using HRM_API.Dtos;
using HRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM_API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/salary-scales")]
    [ApiController]
    public class SalaryScaleController : ControllerBase
    {
        private readonly SalaryScaleService _salaryScaleService;

        public SalaryScaleController(SalaryScaleService salaryScaleService)
        {
            _salaryScaleService = salaryScaleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _salaryScaleService.GetAllAsync();
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSalaryScaleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.Id = id;

            var response = await _salaryScaleService.UpdateAsync(request);

            if (response == null)
                return NotFound(new { message = "Salary scale not found" });

            return Ok(response);
        }
    }
}

