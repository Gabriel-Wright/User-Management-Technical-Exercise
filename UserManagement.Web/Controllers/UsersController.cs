using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.WebMS.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("Test ok");
    }
}
