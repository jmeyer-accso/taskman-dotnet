

using Microsoft.AspNetCore.Mvc;
using Taskman.Models;
using Taskman.Services;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _userService;

    public AuthController(AuthService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto userDto)
    {
        try
        {
            var user = await _userService.RegisterUserAsync(userDto);
            return Ok(new { user.Id, user.Username, user.Email });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
    {
        try
        {
            var token = await _userService.LoginUserAsync(loginDto);
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

}
