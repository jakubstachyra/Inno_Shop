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
        var user = await _userService.RegisterUserAsync(registerUserDto.Name,
            registerUserDto.Email, registerUserDto.Password);
        return CreatedAtAction(nameof(Register), new { id = user.ID });
    }
    [HttpPost("login")]
    public async Task<IActionResult> LoginU(LoginUserDto loginUserDto)
    {
        var token = await _userService.AuthenticateAndGenerateTokenAsync(loginUserDto.Email, loginUserDto.Password);
        
        if(token == null) { return NotFound("Invalid email or password"); }

        return Ok(token);
    }
}
