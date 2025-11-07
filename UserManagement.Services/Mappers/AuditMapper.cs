using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain;

namespace UserManagement.Services.Mappers;

public static class UserAuditMapper
{
    public static UserAudit ToDomainAudit(UserAuditEntity auditEntity)
    {
        return new UserAudit
        {
            Id = auditEntity.Id,
            UserId = auditEntity.UserEntityId,
            UserForename = auditEntity.UserEntity?.Forename ?? "",
            UserSurname = auditEntity.UserEntity?.Surname ?? "",
            UserEmail = auditEntity.UserEntity?.Email ?? "",
            LoggedAt = auditEntity.LoggedAt,
            Action = Enum.Parse<AuditAction>(auditEntity.AuditAction, ignoreCase: true),
            Changes = auditEntity.Changes?
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
