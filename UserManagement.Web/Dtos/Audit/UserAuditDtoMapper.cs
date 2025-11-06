using System.Linq;
using UserManagement.Services.Domain;

namespace UserManagement.Web.Dtos;

public static class UserAuditDtoMapper
{
    public static UserAuditDto ToDto(UserAudit audit)
    {
        return new UserAuditDto
        {
            Id = audit.Id,
            UserId = audit.UserId,
            LoggedAt = audit.LoggedAt,
            Action = audit.Action.ToString(),
            Changes = audit.Changes?.Select(c => new UserAuditChangeDto
            {
                FieldName = c.Change.FieldName.ToString(),
                Before = c.Change.Before,
                After = c.Change.After
            }).ToList() ?? new List<UserAuditChangeDto>()
        };
    }
}