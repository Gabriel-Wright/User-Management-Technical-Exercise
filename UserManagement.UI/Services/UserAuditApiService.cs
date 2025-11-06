using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using UserManagement.UI.Dtos;
using UserManagement.UI.Exceptions;

namespace UserManagement.UI.Services;

public class UserAuditApiService : IUserAuditApiService
{
    private readonly HttpClient _httpClient;

    public UserAuditApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<UserAuditDto>> GetAllAuditsAsync(int page = 1, int pageSize = 10)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString()
        };

        var url = QueryHelpers.AddQueryString("users/audits", queryParams);

        return await GetPagedResultAsync(url);
    }
    public async Task<PagedResult<UserAuditDto>> GetAuditsByUserAsync(long userId, int page = 1, int pageSize = 10)
    {
        if (userId <= 0)
            throw new UserApiException("Invalid user ID.", 400);

        var queryParams = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString()
        };

        var url = QueryHelpers.AddQueryString($"users/audits/{userId}", queryParams);

        return await GetPagedResultAsync(url);
    }
    private async Task<PagedResult<UserAuditDto>> GetPagedResultAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new UserApiException($"Server returned {(int)response.StatusCode}: {errorText}", (int)response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<PagedResult<UserAuditDto>>();
            return result ?? new PagedResult<UserAuditDto> { Items = new List<UserAuditDto>(), TotalCount = 0 };
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Network error fetching audits from {url}: {ex.Message}");
            throw new UserApiException("Network error: unable to reach the server.", inner: ex);
        }
        catch (NotSupportedException ex)
        {
            Console.Error.WriteLine($"Unsupported response fetching audits from {url}: {ex.Message}");
            throw new UserApiException("Server returned an unsupported response format.", inner: ex);
        }
        catch (System.Text.Json.JsonException ex)
        {
            Console.Error.WriteLine($"Malformed JSON response from {url}: {ex.Message}");
            throw new UserApiException("Server returned an unsupported response format.", inner: ex);
        }
    }
}


