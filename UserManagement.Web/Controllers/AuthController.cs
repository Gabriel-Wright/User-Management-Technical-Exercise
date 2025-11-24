using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Domain.Login;

namespace UserManagement.WebMS.Controllers;

[Route("api/authenticate/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        this._authService = authService;
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


