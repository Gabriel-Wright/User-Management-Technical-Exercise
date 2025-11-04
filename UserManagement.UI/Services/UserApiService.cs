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
}