using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Domain.Login;
using LoginRequest = UserManagement.Services.Domain.Login.LoginRequest;

namespace UserManagement.WebMS.Controllers;

[Route("api/authenticate/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        this._authService = authService;
        this._userService = userService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        Log.Information("Login attempt for email: {Email}", request.Email);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.AuthenticateAsync(request.Email, request.Password);

        if (result == null)
        {
            Log.Warning("Login failed for email: {Email}", request.Email);
            return Unauthorized(new { message = "Invalid email or password" });
        }

        Log.Information("Login successful for email: {Email}", request.Email);
        return Ok(result);
    }

}


