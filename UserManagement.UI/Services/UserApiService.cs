using System.Net.Http.Json;
using UserManagement.UI.Dtos;

namespace UserManagement.UI.Services;

public class UserApiService
{
    private readonly HttpClient _httpClient; public UserApiService(HttpClient http) { _httpClient = http; }
    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<UserDto>>("users") ?? new List<UserDto>();
    }

    public async Task<(List<UserDto> Users, int TotalCount)> GetUsersByQueryAsync(
        string? searchTerm = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 10,
        string sortBy = "id",
        bool sortDescending = false)
    {
        var query = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}",
            $"sortBy={sortBy}",
            $"sortDescending={sortDescending.ToString().ToLower()}"
        };

        if (!string.IsNullOrWhiteSpace(searchTerm)) query.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");

        if (isActive.HasValue) query.Add($"isActive={isActive.Value.ToString().ToLower()}");

        var url = $"users/query?{string.Join("&", query)}";

        var response = await _httpClient.GetAsync(url);
        //Not sure about this - need to have a think
        if (!response.IsSuccessStatusCode) return (new List<UserDto>(), 0);

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        return (users ?? new List<UserDto>(), users?.Count ?? 0);
    }

    public async Task DeleteUserAsync(long userId)
    {
        var response = await _httpClient.DeleteAsync($"users/{userId}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Delete failed: {error}");
        }
    }

    public async Task<UserDto> PatchUserAsync(long id, UserPatchDto patchDto)
    {
        var response = await _httpClient.PatchAsJsonAsync($"users/{id}", patchDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>() ?? throw new Exception("Failed update user");
    }

    public async Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto)
    {
        {
            var response = await _httpClient.PostAsJsonAsync("users", userCreateDto);

            response.EnsureSuccessStatusCode();

            var createdUser = await response.Content.ReadFromJsonAsync<UserDto>()
                ?? throw new Exception("Failed to create user");

            return createdUser;
        }

    }
}