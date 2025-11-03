using System;
using UserManagement.Models;
using UserManagement.Services.Domain;
using UserManagement.Services.Mappers;

public class AuditMapperTests
{
    [Fact]
    public void MapToDomainAudit_ShouldConvertAuditEntityToDomainAudit()
    {
        var auditEntity = new AuditEntity
        {
            Id = 1,
            EntityId = 100,
            Timestamp = new DateTime(2023, 1, 1),
            AuditAction = "Created",
            ChangedBy = "Greg Heffley",
            Changes = "Initial creation"
        };

        var audit = AuditMapper.ToUserEntity(auditEntity);

        // Assert
        audit.Id.Should().Be(1);
        audit.EntityId.Should().Be(100);
        audit.Timestamp.Should().Be(new DateTime(2023, 1, 1));
        audit.Action.Should().Be(AuditAction.Created);
        audit.ChangedBy.Should().Be("Greg Heffley");
        audit.Changes.Should().Be("Initial creation");
    }

    [Fact]
    public void MapToEntity_ShouldConvertDomainAuditToAuditEntity()
    {
        var audit = new Audit
        {
            Id = 2,
            EntityId = 200,
            Timestamp = new DateTime(2024, 2, 2),
            Action = AuditAction.Updated,
            ChangedBy = "Rodrick Heffley",
            Changes = "Updated email"
        };

        var auditEntity = AuditMapper.ToAuditEntity(audit);

        // Assert
        auditEntity.Id.Should().Be(2);
        auditEntity.EntityId.Should().Be(200);
        auditEntity.Timestamp.Should().Be(new DateTime(2024, 2, 2));
        auditEntity.AuditAction.Should().Be("Updated");
        auditEntity.ChangedBy.Should().Be("Rodrick Heffley");
        auditEntity.Changes.Should().Be("Updated email");
    }

}
