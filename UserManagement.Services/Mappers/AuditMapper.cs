using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain;

namespace UserManagement.Services.Mappers;

public static class UserAuditMapper
{
    //
    public static UserAudit ToDomainAudit(UserAuditEntity userAuditEntity, List<UserAuditChangeEntity>? userAuditChangeEntities)
    {
        UserAudit userAudit = new UserAudit();
        return new UserAudit
        {
            Id = userAuditEntity.Id,
            UserId = userAuditEntity.UserEntityId,
            LoggedAt = userAuditEntity.LoggedAt,
            Action = Enum.Parse<AuditAction>(userAuditEntity.AuditAction, ignoreCase: true),
            Changes = userAuditChangeEntities?
                    .Select(ToDomainAuditChange)
                    .ToList() ?? new List<UserAuditChange>()
        };
    }

    public static UserAuditChange ToDomainAuditChange(UserAuditChangeEntity changeEntity)
    {
        return new UserAuditChange
        {
            Id = changeEntity.Id,
            AuditId = changeEntity.AuditId,
            Change = new UserAuditFieldChange
            {
                FieldName = Enum.Parse<UserField>(changeEntity.Field, ignoreCase: true),
                Before = changeEntity.Before,
                After = changeEntity.After
            }

        };
    }
}
