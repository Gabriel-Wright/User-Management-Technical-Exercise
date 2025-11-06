using UserManagement.UI.Dtos;

namespace UserManagement.UI.Services;

public interface IUserAuditApiService
{
    Task<PagedResult<UserAuditDto>> GetAllAuditsAsync(int page = 1, int pageSize = 10);
    Task<PagedResult<UserAuditDto>> GetAuditsByUserAsync(long userId, int page = 1, int pageSize = 10);
}