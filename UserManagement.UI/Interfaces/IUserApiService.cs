using UserManagement.UI.Dtos;

public interface IUserApiService
{
    Task<(List<UserDto> Users, int TotalCount)> GetUsersByQueryAsync(
        string? searchTerm = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 10,
        string sortBy = "id",
        bool sortDescending = false);

    Task DeleteUserAsync(long userId);
    Task SoftDeleteUserAsync(long userId);
    Task<UserDto> PatchUserAsync(long id, UserPatchDto patchDto);
    Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto);
}