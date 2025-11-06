using System.Linq;
using System.Threading.Tasks;
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
        (IEnumerable<UserAudit> audits, int totalCount) = await auditService.GetAllUserAudits(page, pageSize);

        if (audits == null || !audits.Any())
            return NotFound("No audits found.");

        var result = new PagedResult<UserAuditDto>
        {
            Items = audits.Select(UserAuditDtoMapper.ToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Get audits for a specific user (paginated)
    /// </summary>
    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetUserAudits(long userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (userId <= 0)
            return BadRequest("Invalid user ID.");

        (IEnumerable<UserAudit> audits, int totalCount) = await auditService.GetAllUserAuditsById(userId, page, pageSize);

        if (audits == null || !audits.Any())
            return NotFound($"No audits found for user {userId}.");

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
