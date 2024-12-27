using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;

namespace UserManagement.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
    {
        var user = await _userService.RegisterUserAsync(registerUserDto);
        return CreatedAtAction(nameof(Register), new { id = user.ID });
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto loginDto)
    {
        try
        {
            var token = await _userService.AuthenticateAndGenerateTokenAsync(loginDto.Email, loginDto.Password);
            return Ok(new { token });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        try
        {
            await _userService.ConfirmEmailAsync(token);
            return Ok(new { message = "Account confirmed successfully!" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteUser(int id)
    {
        try
        {
            await _userService.SoftDeleteUserAsync(id);
            return Ok(new { message = "User soft deleted successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

}
