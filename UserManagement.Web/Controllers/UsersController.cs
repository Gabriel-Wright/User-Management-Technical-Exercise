using System.Linq;
using System.Threading.Tasks;
using Serilog;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Dtos;

namespace UserManagement.WebMS.Controllers;

/// <summary>
/// This UserController is a class detailing API endpoints needed to retrieve data from Service Layer.
/// Data is passed to these API endpoints from UI via Dtos, these Dtos will be input validated on the UI,
/// however I have also validated them here as there could be man in the middle interference, and I have also
/// validated again with more business specific logic on the service layer. The error messages given when input validation
/// i.e. format of JSON from Dto fails - are quite generic, since UI layer should cover this, if this project
/// was to be expanded though - more detailed error messaging here would be good.
/// 
/// Currently Endpoints are fixed - if expanded it might be better to have a filter based API so the end points do 
/// not have to be so specific. But for the current purposes this serves the specification. 
/// </summary>

[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        if (users == null || !users.Any()) return NotFound("No users found.");
        var dtos = users.Select(u => UserDtoMapper.ToDto(u));
        return Ok(dtos);
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetUsersByStatus([FromQuery] bool isActive)
    {
        var users = await _userService.FilterByActiveAsync(isActive);
        if (users == null || !users.Any()) return NotFound($"No {(isActive ? "active" : "inactive")} users found.");

        var dtos = users.Select(UserDtoMapper.ToDto);
        return Ok(dtos);
    }

    // Actually this end point seems kind of annoying - since you need both ? 
    // Will refactor this in future
    [HttpGet("search")]
    public async Task<IActionResult> GetUsersByName([FromQuery] string forename, [FromQuery] string surname)
    {
        if (string.IsNullOrWhiteSpace(forename) || string.IsNullOrWhiteSpace(surname))
            return BadRequest("Both forename and surname are required.");

        var users = await _userService.GetByNameAsync(forename, surname);

        if (users == null || !users.Any())
            return NotFound($"No users found with name {forename} {surname}.");

        var dtos = users.Select(UserDtoMapper.ToDto);
        return Ok(dtos);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetUsersById(long id)
    {
        if (id <= 0) return BadRequest("User ID < 0, invalid Id.");

        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound($"User with ID {id} not found.");

        var dto = UserDtoMapper.ToDto(user);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody] UserCreateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            Log.Warning("{@ModelState}", ModelState);
            return BadRequest("In valid data used for creation.");
        }

        var user = UserToUserCreateDtoMapper.ToUser(createDto);
        var createdUser = await _userService.AddUserAsync(user);
        await _userService.SaveAsync();

        var resultDto = UserDtoMapper.ToDto(createdUser);
        return CreatedAtAction(nameof(GetUsersById), new { id = resultDto.Id }, resultDto);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateUserPut(long id, [FromBody] UserDto dto)
    {
        if (!ModelState.IsValid)
        {
            Log.Warning("{@ModelState}", ModelState);
            return BadRequest("Invalid data used for put.");
        }

        if (id != dto.Id)
            return BadRequest("Mismatched IDs for update");

        var userToUpdate = UserDtoMapper.ToUser(dto);
        var updatedUser = await _userService.UpdateUserAsync(userToUpdate);
        await _userService.SaveAsync();

        var updatedDto = UserDtoMapper.ToDto(updatedUser);
        return Ok(updatedDto);
    }

    [HttpPatch("{id:long}")]
    public async Task<IActionResult> UpdateUserPatch(long id, [FromBody] UserPatchDto patchDto)
    {
        if (!ModelState.IsValid)
        {
            Log.Warning("{@ModelState}", ModelState);
            return BadRequest("Invalid data used for Patch");
        }

        if (patchDto == null)
            return BadRequest("Patch data is required.");

        var existingUser = await _userService.GetByIdAsync(id);
        if (existingUser == null)
            return NotFound($"User with ID {id} not found.");

        UserPatchApplier.ApplyPatch(existingUser, patchDto);

        var updatedUser = await _userService.UpdateUserAsync(existingUser);
        await _userService.SaveAsync();

        var resultDto = UserDtoMapper.ToDto(updatedUser);
        return Ok(resultDto);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        if (id <= 0)
            return BadRequest("Invalid user ID.");

        await _userService.DeleteUserAsync(id);
        await _userService.SaveAsync();

        return NoContent(); //204 is standard successful DELETE - no body returned
    }

}
