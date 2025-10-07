using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HumanResourceManagement.Models;
using HumanResourceManagement.Dtos.Auth;
using HumanResourceManagement.Dtos.Common;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HumanResourceManagement.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Step 1: Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
        {
            return Unauthorized(new ErrorResponse 
            { 
                Message = "Invalid credentials" 
            });
        }

        // Step 2: Sign out any existing session first
        await _signInManager.SignOutAsync();

        // Step 3: Attempt to sign in with password
        var result = await _signInManager.PasswordSignInAsync(
            user, 
            request.Password, 
            isPersistent: false,      // Cookie not persists after browser close
            lockoutOnFailure: true   // Enable lockout after 5 failed attempts
        );

        if (result.Succeeded)
        {
            // Get user roles from AspNetUserRoles table
            var roles = await _userManager.GetRolesAsync(user);
            
            // IMPORTANT: User MUST have a role assigned
            if (roles == null || !roles.Any())
            {
                return StatusCode(500, new ErrorResponse
                { 
                    Message = "User configuration error.",
                    ErrorCode = "NO_ROLE_ASSIGNED"
                });
            }

            // Return Dto
            var response = new LoginResponse
            {
                Success = true,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Role = roles.First()
                }
            };

            return Ok(response);
        }

        // HTTP 423 Locked: Account locked due to > 5 failed attempts
        if (result.IsLockedOut)
        {
            return StatusCode(423, new ErrorResponse
            { 
                Message = "Account is locked due to multiple failed login attempts. Please try again later.",
                ErrorCode = "ACCOUNT_LOCKED"
            });
        }

        return Unauthorized(new ErrorResponse 
        { 
            Message = "Invalid credentials" 
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Logged out successfully"
        });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        // Extract user ID from authentication cookie 
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(
                new ErrorResponse 
                { 
                    Message = "User ID not found in authentication token" 
                }
            );
        }
        
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new ErrorResponse 
            { 
                Message = "User not found" 
            });
        }

        var roles = await _userManager.GetRolesAsync(user);
        
        // Validate user has a role
        if (roles == null || !roles.Any())
        {
            return StatusCode(500, new ErrorResponse
            { 
                Message = "User configuration error.",
                ErrorCode = "NO_ROLE_ASSIGNED"
            });
        }
        
        // Return UserDto
        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = roles.First()
        };

        return Ok(userDto);
    }

    [HttpGet("unauthorized")]
    public IActionResult UnauthorizedEndpoint()
    {
        return StatusCode(401, new ErrorResponse 
        { 
            Message = "Authentication required" 
        });
    }

    [HttpGet("forbidden")]
    public IActionResult ForbiddenEndpoint()
    {
        return StatusCode(403, new ErrorResponse 
        { 
            Message = "Access denied" 
        });
    }
}