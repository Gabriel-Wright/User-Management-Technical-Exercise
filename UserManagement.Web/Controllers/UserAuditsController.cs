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

[Route("api/users/audits")]
public class UserAuditsController : ControllerBase
{
    private readonly IAuditService auditService;

    public UserAuditsController(IAuditService auditService)
    {
        this.auditService = auditService;
    }

    /// <summary>
    /// Get all audits (paginated)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAudits([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        Log.Information("Fetching all audits for page {page} of size {size}", page, pageSize);
        (IEnumerable<UserAudit> audits, int totalCount) = await auditService.GetAllUserAudits(page, pageSize);

        var result = new PagedResult<UserAuditDto>
        {
            Items = audits.Select(UserAuditDtoMapper.ToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
        return Ok(result);
    }

    [HttpGet("query")]
    public async Task<IActionResult> GetUsersByQuery([FromQuery] UserAuditQueryDto queryDto)
    {
        AuditAction? action = null;
        if (!string.IsNullOrWhiteSpace(queryDto.Action))
            action = Enum.Parse<AuditAction>(queryDto.Action, ignoreCase: true);

        {
            var query = new UserAuditQuery
            {
                Page = queryDto.Page,
                PageSize = queryDto.PageSize,
                SearchTerm = queryDto.SearchTerm,
                Action = action,
                // No sorting implemented yet
            };
            (IEnumerable<UserAudit> audits, int totalCount) = await auditService.GetAuditsByQueryAsync(query);
            var result = new PagedResult<UserAuditDto>
            {
                Items = audits.Select(UserAuditDtoMapper.ToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = query.Page,
                PageSize = query.PageSize
            };

            return Ok(result);
        }
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

        (IEnumerable<UserAudit> audits, int totalCount) = await auditService.GetAllUserAuditsById(userId, page, pageSize);

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
