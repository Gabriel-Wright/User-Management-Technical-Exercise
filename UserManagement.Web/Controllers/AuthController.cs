using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain;
using UserManagement.Services.Domain.Login;
using UserManagement.Web.Dtos;

namespace UserManagement.WebMS.Controllers;

[Route("api/authenticate/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IDataContext _dataContext; // for now

    public AuthController(IDataContext dataContext)
    {
        this._dataContext = dataContext;
    }
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> AuthenticateAsync(string email, string password)
    {
        var user = await _dataContext.GetAll<UserEntity>().FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return BadRequest("no user"); //obvs combine these for full releasae - just for testing rn

        if (new PasswordHasher<UserEntity>().VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
        {
            return BadRequest("wrong pass");
        }

        return Ok(new LoginResponse
        {
            Token = "blah",
            Email = user.Email,
            Role = user.UserRole,
            Expiration = DateTime.UtcNow.AddMinutes(60)
        });
    }

}


