using System;
using UserManagement.Models;
using UserManagement.Services.Domain;

namespace UserManagement.Services.Mappers;

public static class AuditMapper
{
    public static Audit ToUserEntity(AuditEntity audit)
    {
        return new Audit
        {
            Id = audit.Id,
            EntityId = audit.EntityId,
            Timestamp = audit.Timestamp,
            Action = Enum.Parse<AuditAction>(audit.AuditAction, ignoreCase: true),
            ChangedBy = audit.ChangedBy,
            Changes = audit.Changes
        };
    }

    public static AuditEntity ToAuditEntity(Audit audit)
    {
        return new AuditEntity
        {
            Id = audit.Id,
            EntityId = audit.EntityId,
            Timestamp = audit.Timestamp,
            AuditAction = audit.Action.ToString(),
            ChangedBy = audit.ChangedBy,
            Changes = audit.Changes
        };
    }
}