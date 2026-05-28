using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.Models.DTOs.UserAuths;

namespace NovaStaff.API.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // =========================================================
    // CREATE USER (ADMIN)
    // =========================================================
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var id = await _userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // =========================================================
    // GET MY PROFILE
    // =========================================================
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await _userService.GetMyProfileAsync();
        return Ok(result);
    }

    // =========================================================
    // GET USER BY ID (ADMIN)
    // =========================================================
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        // bạn có thể thêm method GetByIdAsync nếu cần
        return Ok(new { message = "Implement later if needed" });
    }

    // =========================================================
    // CHANGE PASSWORD (SELF)
    // =========================================================
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        await _userService.ChangePasswordAsync(request);
        return NoContent();
    }

    // =========================================================
    // UPDATE ROLE (ADMIN)
    // =========================================================
    [HttpPut("{id}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        await _userService.UpdateRoleAsync(id, request.Role);
        return NoContent();
    }

    // =========================================================
    // LOCK USER (ADMIN)
    // =========================================================
    [HttpPut("{id}/lock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Lock(int id)
    {
        await _userService.LockAsync(id);
        return NoContent();
    }

    // =========================================================
    // UNLOCK USER (ADMIN)
    // =========================================================
    [HttpPut("{id}/unlock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Unlock(int id)
    {
        await _userService.UnlockAsync(id);
        return NoContent();
    }
    // =========================================================
    // RESET PASSWORD (ADMIN)
    // =========================================================
    [HttpPost("{id}/reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var newPassword = await _userService.ResetPasswordAsync(id);
        return Ok(new { password = newPassword });
    }
}