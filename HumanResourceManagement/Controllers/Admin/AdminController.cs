using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HumanResourceManagement.Dtos.Dashboard;
using HumanResourceManagement.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using HRM_API.Dtos.Dashboard.Policy;
using Microsoft.AspNetCore.Identity;

namespace HumanResourceManagement.Controllers.Admin;

[Route("api/admin")]
[Authorize(Roles = "Admin")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly HRMDbContext _dbContext;
    private readonly IMapper _mapper;

    public AdminController(HRMDbContext dbContext, IMapper mapper, UserManager<User> userManager) 
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _userManager = userManager;
    }

    [HttpGet("dashboard")]
    public IActionResult GetAdminDashboard()
    {
        var dashboard = new DashboardDto
        {
            Role = "Admin",
            LoginTime = DateTime.Now
        };

        return Ok(dashboard);
    }

    [HttpGet("policy")]
    public async Task<IActionResult> GetPolicy()
    {
        try 
        {
            var policy = await _dbContext.Policies.FirstAsync();

            if (policy == null)
            {
                return NotFound("No policy");
            }
            
            var policyDto = _mapper.Map<PolicyDto>(policy);

            return Ok(policyDto);
        }
        catch 
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An internal server error occurred. Please try again later.");
        }
    }

    [HttpPut("policy/{id}")]
    public async Task<IActionResult> UpdatePolicy(int id, UpdatePolicyDto newPolicy)
    {
        try 
        {
            var toUpdate = await _dbContext.Policies.FirstOrDefaultAsync(p => p.Id == id);

            if (toUpdate == null || toUpdate.Id != id)
            {
                return NotFound("No policy");
            }
            
            await _dbContext.Policies
                .Where(p => p.Id == toUpdate.Id)
                .ExecuteUpdateAsync(
                    policy => policy
                    .SetProperty(p => p.WorkStartTime, policy => newPolicy.WorkStartTime)
                    .SetProperty(p => p.WorkEndTime, policy => newPolicy.WorkEndTime)
                    .SetProperty(p => p.LateEarlyThresholdMinutes, policy => newPolicy.LateEarlyThresholdMinutes)
                    .SetProperty(p => p.LateEarlyDeductionPercent, policy => newPolicy.LateEarlyDeductionPercent)
                    .SetProperty(p => p.MonthlyOvertimeHoursLimit, policy => newPolicy.MonthlyOvertimeHoursLimit)
                    .SetProperty(p => p.AnnualLeaveMaxDays, policy => newPolicy.AnnualLeaveMaxDays)
                    );

            var updatedPolicyDto = _mapper.Map<PolicyDto>(newPolicy);

            return Ok(updatedPolicyDto);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An internal server error occurred. Please try again later.");
        } 
    } 

    // [HttpGet("users")]
    // public async Task<IActionResult> GetUsers()
    // {
    //     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    //     if (string.IsNullOrEmpty(userId))
    //     {
    //         return BadRequest(
    //             new ErrorResponse 
    //             { 
    //                 Message = "User ID not found in authentication token" 
    //             }
    //         );
    //     }

    //     try
    //     {
    //         var userDtos = new List<UserDto>();
    //         var users = await _dbContext.Users.ToListAsync();

    //         foreach (var user in users)
    //         {
    //             userDtos.Add(new UserDto 
    //             {
    //                 Id = userId,
    //                 FullName = user.FullName,
    //                 Address = user.Address,
    //                 ProfileImgUrl = user.ProfileImgUrl,
    //                 Salary = user.Salary,
    //                 LeaveDaysTaken = await _dbContext.
    //             });
    //         }
    //     }
    //     catch (Exception ex)
    //     {

    //     }
    // }
}