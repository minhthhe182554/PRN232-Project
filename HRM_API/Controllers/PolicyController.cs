using HRM_API.Dtos;
using HRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM_API.Controllers
{
    [Authorize]
    [Route("api/policies")]
    [ApiController]
    public class PolicyController : ControllerBase
    {
        private readonly PolicyService _policyService;

        public PolicyController(PolicyService policyService)
        {
            _policyService = policyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _policyService.GetAll();

            if (response == null) 
                return BadRequest(new { message = "No policies" });

            return Ok(response);    
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePolicyRequest request)
        {
            // Ensure Id from route matches request
            request.Id = id;

            var response = await _policyService.UpdateAsync(request);

            if (response == null)
                return NotFound(new { message = "Policy not found" });
            
            return Ok(response);
        }
    }
}
