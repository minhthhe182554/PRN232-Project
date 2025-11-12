using HRM_API.Dtos;
using HRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM_API.Controllers
{
    [ApiController]
    [Route("api/")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserService _userService;

        public AdminController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<UserList>> GetAllUsers()
        {
            var userList = await _userService.GetAllUsersAsync();
            return Ok(userList);
        }

        // [HttpPatch("{id}")]
        // public async Task<IActionResult> BanUser(int id)
        // {
        //     var succeed = await _userService.BanUser(id);

        //     if (succeed) return Ok();
        //     else return BadRequest();
        // }
    }
}