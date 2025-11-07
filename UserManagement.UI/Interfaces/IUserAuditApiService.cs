using UserManagement.UI.Dtos;

namespace UserManagement.UI.Services;

public interface IUserAuditApiService
{
    Task<PagedResult<UserAuditDto>> GetAuditsByUserAsync(long userId, int page = 1, int pageSize = 10);
    Task<PagedResult<UserAuditDto>> GetAuditsByQueryAsync(
        string? searchTerm = null,
        string? action = null,
        int page = 1,
        int pageSize = 10);
}