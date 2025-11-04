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

    /// <summary>
    /// Standard Get All Endpoint - unordered
    /// </summary>
    /// <returns>Unordered list of all users</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        if (users == null || !users.Any()) return NotFound("No users found.");
        var dtos = users.Select(u => UserDtoMapper.ToDto(u));
        return Ok(dtos);
    }

    /// <summary>
    /// Gets all users that are either active or not active
    /// </summary>
    /// <param name="isActive">Are users active or not</param>
    /// <returns>Either all active or inactive users</returns>
    [HttpGet("status")]
    public async Task<IActionResult> GetUsersByStatus([FromQuery] bool isActive)
    {
        var users = await _userService.FilterByActiveAsync(isActive);
        if (users == null || !users.Any()) return NotFound($"No {(isActive ? "active" : "inactive")} users found.");

        var dtos = users.Select(UserDtoMapper.ToDto);
        return Ok(dtos);
    }

    /// <summary>
    /// Very specific search operation - end point searching by both forename, and surname.
    /// Way too specific - need to redesign.
    /// </summary>
    /// <param name="forename">First name of user - must be specified</param>
    /// <param name="surname">Surname of user - also must be specified</param>
    /// <returns>All users of that specific first name and surname</returns>
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

    /// <summary>
    /// Gets the specific user with id `id`.
    /// </summary>
    /// <param name="id">The expected id of the user</param>
    /// <returns>User with id passed, or if no such user exists - returns null</returns>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetUsersById(long id)
    {
        if (id <= 0) return BadRequest("User ID < 0, invalid Id.");

        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound($"User with ID {id} not found.");

        var dto = UserDtoMapper.ToDto(user);
        return Ok(dto);
    }

    /// <summary>
    /// Create a new user via this endpoint
    /// </summary>
    /// <param name="createDto">Dto detailing parameters / attributes of this new user</param>
    /// <returns>The user you just created - if created successfully</returns>
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

    /// <summary>
    /// Updates the user of id 'id' with a fully populated user dto, essentially replaces that user.
    /// </summary>
    /// <param name="id">Id of the user you wish to replace</param>
    /// <param name="dto">Information of the user that you wish to put in place of the user of id `id`.</param>
    /// <returns>If successful - returns the replaced user.</returns>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateUserPut(long id, [FromBody] UserDto dto)
    {
        if (!ModelState.IsValid)
        {
            Log.Warning("{@ModelState}", ModelState);
            return BadRequest("Invalid data used for put.");
        }

        if (id != dto.Id) return BadRequest("Mismatched IDs for update");

        var userToUpdate = UserDtoMapper.ToUser(dto);
        var updatedUser = await _userService.UpdateUserAsync(userToUpdate);
        await _userService.SaveAsync();

        var updatedDto = UserDtoMapper.ToDto(updatedUser);
        return Ok(updatedDto);
    }

    /// <summary>
    /// Update details user of id `id` for specific attributes using a patchDto
    /// </summary>
    /// <param name="id">Id of user you wish to update</param>
    /// <param name="patchDto">Specific details of user that you wish to update, do not have to specify everything.</param>
    /// <returns>If successful - returns user of same Id with attribute changed from that patchDto</returns>
    [HttpPatch("{id:long}")]
    public async Task<IActionResult> UpdateUserPatch(long id, [FromBody] UserPatchDto patchDto)
    {

        if (patchDto == null) return BadRequest("Patch data is required.");

        if (!ModelState.IsValid)
        {
            Log.Warning("{@ModelState}", ModelState);
            return BadRequest("Invalid data used for Patch");
        }

        var existingUser = await _userService.GetByIdAsync(id);
        if (existingUser == null)
            return NotFound($"User with ID {id} not found.");

        UserPatchApplier.ApplyPatch(existingUser, patchDto);

        var updatedUser = await _userService.UpdateUserAsync(existingUser);
        await _userService.SaveAsync();

        var resultDto = UserDtoMapper.ToDto(updatedUser);
        return Ok(resultDto);
    }

    /// <summary>
    /// Deletes the user of id `id`.
    /// </summary>
    /// <param name="id">`id` of the user you wish to delete</param>
    /// <returns>Null - no user to return. 204 if successful</returns>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        if (id <= 0) return BadRequest("Invalid user ID.");

        await _userService.DeleteUserAsync(id);
        await _userService.SaveAsync();

        return NoContent(); //204 is standard successful DELETE - no body returned
    }

}
