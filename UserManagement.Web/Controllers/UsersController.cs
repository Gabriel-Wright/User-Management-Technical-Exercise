using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Serilog;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web;
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
    private readonly IAuthService _authService;
    public UsersController(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    /// <summary>
    /// Standard Get All Endpoint - unordered
    /// </summary>
    /// <returns>Unordered list of all users</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        Log.Information("Fetching all users on GET api/users");
        var users = await _userService.GetAllAsync();
        if (users == null || !users.Any()) return NotFound("No users found.");
        var dtos = users.Select(u => UserDtoMapper.ToDto(u));
        return Ok(dtos);
    }

    /// <summary>
    /// Get All users that match query passed to it. I.E can search for specific keywords,
    /// whether a user is active here -> also sort how large you want the query to be, how many
    /// and it what order you want the users returned.
    /// </summary>
    /// <param name="queryDto"></param>
    /// <returns></returns>
    [HttpGet("query")]
    public async Task<IActionResult> GetUsersByQuery([FromQuery] UserQueryDto queryDto)
    {
        Log.Information("Fetching users by query on GET api/users/query");
        var query = new UserQuery
        {
            Page = queryDto.Page,
            PageSize = queryDto.PageSize,
            SortBy = queryDto.SortBy ?? "Id",
            SortDescending = queryDto.SortDescending,
            IsActive = queryDto.IsActive,
            SearchTerm = queryDto.SearchTerm
        };
        var (users, totalCount) = await _userService.GetUsersAsync(query);

        var result = new PagedResult<UserDto>
        {
            Items = users.Select(UserDtoMapper.ToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = query.Page,
            PageSize = query.PageSize
        };

        var dtos = users.Select(UserDtoMapper.ToDto);
        return Ok(result);
    }

    /// <summary>
    /// Gets the specific user with id `id`.
    /// </summary>
    /// <param name="id">The expected id of the user</param>
    /// <returns>User with id passed, or if no such user exists - returns null</returns>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetUsersById(long id)
    {
        Log.Information("Fetching user by ID {id} on GET api/users/{id}", id);
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
        Log.Information("Creating new user on POST api/users with data {@createDto}", createDto);
        if (!ModelState.IsValid)
        {
            //Could have more detail here
            Log.Warning("{@ModelState}", ModelState);
            return BadRequest("Invalid data used for creation.");
        }

        var user = UserToUserCreateDtoMapper.ToUser(createDto);
        var createdUser = await _userService.AddUserAsync(user);

        //set default password
        await _authService.SetDefaultUserPasswordAsync(createdUser);


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
        Log.Information("Updating user of ID {id} on PUT api/users/{id} with data {@dto}", id, dto);
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
        Log.Information("Patching user of ID {id} on PATCH api/users/{id} with data {@patchDto}", id, patchDto);
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

        var resultDto = UserDtoMapper.ToDto(updatedUser);
        return Ok(resultDto);
    }

    /// <summary>
    /// Soft deletes the user of id `id`. This means the user won't appear in any queries,
    /// but is not removed from the database.
    /// </summary>
    /// <param name="id">`id` of user you wish to soft delete</param>
    /// <returns>Null - no user to return. 204 if successful</returns>
    [HttpDelete("soft/{id:long}")]
    public async Task<IActionResult> SoftDeleteUser(long id)
    {
        Log.Information("Soft deleting user of ID {id} on DELETE api/users/soft/{id}", id);
        if (id <= 0) return BadRequest("Invalid user ID.");

        await _userService.SoftDeleteUserAsync(id);

        return NoContent();
    }
}
