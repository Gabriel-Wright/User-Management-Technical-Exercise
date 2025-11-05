using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using UserManagement.UI.Dtos;
using UserManagement.UI.Exceptions;

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
        var queryParams = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["sortBy"] = sortBy,
            ["sortDescending"] = sortDescending.ToString().ToLower(),
            ["searchTerm"] = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
            ["isActive"] = isActive?.ToString().ToLower()
        };

        var url = QueryHelpers.AddQueryString("users/query", queryParams);

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new UserApiException($"Server returned {(int)response.StatusCode}: {errorText}", (int)response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>();

            if (result == null)
                return (new List<UserDto>(), 0);

            return (result.Items, result.TotalCount);
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Network error fetching users from {url}: {ex.Message}");
            throw new UserApiException("Network error: unable to reach the server.", inner: ex);
        }
        catch (NotSupportedException ex)
        {
            Console.Error.WriteLine($"Unsupported response fetching users from {url}: {ex.Message}");
            throw new UserApiException("Server returned an unsupported response format.", inner: ex);
        }
        catch (System.Text.Json.JsonException ex)
        {
            Console.Error.WriteLine($"Malformed JSON response from {url}: {ex.Message}");
            throw new UserApiException("Server returned an unsupported response format.", inner: ex);
        }
    }

    public async Task DeleteUserAsync(long userId)
    {
        var response = await _httpClient.DeleteAsync($"users/{userId}");

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            throw new UserApiException(
                $"Server returned {(int)response.StatusCode}: {errorText}",
                (int)response.StatusCode);
        }
    }
    public async Task<UserDto> PatchUserAsync(long id, UserPatchDto patchDto)
    {
        var response = await _httpClient.PatchAsJsonAsync($"users/{id}", patchDto);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            throw new UserApiException(
                $"Server returned {(int)response.StatusCode}: {errorText}",
                (int)response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<UserDto>();
        if (result == null)
            throw new UserApiException("Server returned empty response.", 500);

        return result;
    }

    public async Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto)
    {
        var response = await _httpClient.PostAsJsonAsync("users", userCreateDto);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            throw new UserApiException(
                $"Server returned {(int)response.StatusCode}: {errorText}",
                (int)response.StatusCode);
        }

        //Not sure how to handle this case in UI - but I think it's worth catching
        var createdUser = await response.Content.ReadFromJsonAsync<UserDto>()
            ?? throw new UserApiException("Server returned empty user object after creation.");

        return createdUser;
    }
}