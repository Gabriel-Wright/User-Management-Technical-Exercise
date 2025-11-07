using System;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using UserManagement.Services.Domain;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web;
using UserManagement.Web.Dtos;

/// <summary>
/// Controller for managing user audits
/// Less authentication here as if audits are created, they are from valid users
/// trust our DB in this instance more so than user data endpoints.
/// </summary>
[Route("api/users/audits")]
public class UserAuditsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public UserAuditsController(IAuditService auditService)
    {
        this._auditService = auditService;
    }

    /// <summary>
    /// Get audits by query (paginated)
    /// </summary>
    [HttpGet("query")]
    public async Task<IActionResult> GetUsersByQuery([FromQuery] UserAuditQueryDto queryDto)
    {

        AuditAction? action = null;
        if (!string.IsNullOrWhiteSpace(queryDto.Action))
            action = Enum.Parse<AuditAction>(queryDto.Action, ignoreCase: true);


        var query = new UserAuditQuery
        {
            Page = queryDto.Page,
            PageSize = queryDto.PageSize,
            SearchTerm = queryDto.SearchTerm,
            Action = action,
            //No sorting implemented
        };
        (IEnumerable<UserAudit> audits, int totalCount) = await _auditService.GetAuditsByQueryAsync(query);
        var result = new PagedResult<UserAuditDto>
        {
            Items = audits.Select(UserAuditDtoMapper.ToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = query.Page,
            PageSize = query.PageSize
        };

        return Ok(result);

    }


    /// <summary>
    /// Get audits for a specific user (paginated)
    /// </summary>
    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetUserAudits(long userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        Log.Information("Feching all audits for user of Id {id} for page {page} of size {size}", userId, page, pageSize);

        if (userId <= 0)
            return BadRequest("Invalid user ID.");

        (IEnumerable<UserAudit> audits, int totalCount) = await _auditService.GetAllUserAuditsById(userId, page, pageSize);

        var result = new PagedResult<UserAuditDto>
        {
            Items = audits.Select(UserAuditDtoMapper.ToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };

        return Ok(result);
    }
}
