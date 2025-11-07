using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain;
using UserManagement.Services.Mappers;

public class AuditMapperTests
{
    [Fact]
    public void MapToDomainAudit_WhenNoAuditChanges_ShouldConvertAuditEntityToDomainAudit()
    {
        var auditEntity = new UserAuditEntity
        {
            Id = 1,
            UserEntityId = 100,
            UserEntity = new UserEntity { Id = 1, Forename = "Nav", Surname = "Test", Email = "nav@test.com" },
            LoggedAt = new DateTime(2023, 1, 1),
            AuditAction = "Deleted",
        };

        var audit = UserAuditMapper.ToDomainAudit(auditEntity);

        audit.Id.Should().Be(1);
        audit.UserId.Should().Be(100);
        audit.LoggedAt.Should().Be(new DateTime(2023, 1, 1));
        audit.Action.Should().Be(AuditAction.Deleted);
        audit.Changes.Should().BeEmpty();
    }

    [Fact]
    public void MapToDomainAudit_WhenAuditChanges_ShouldConvertAuditEntityToDomainAudit()
    {
        var auditEntity = new UserAuditEntity
        {
            Id = 1,
            UserEntityId = 100,
            UserEntity = new UserEntity(),
            LoggedAt = new DateTime(2023, 1, 1),
            AuditAction = "Deleted",
        };

        var auditChangeEntity = new UserAuditChangeEntity
        {
            Id = 1,
            AuditId = 1,
            Field = "Forename",
            Before = "Jonty",
            After = "Jon"
        };

        var auditChanges = new List<UserAuditChangeEntity> { auditChangeEntity };
        auditEntity.Changes = auditChanges;
        var audit = UserAuditMapper.ToDomainAudit(auditEntity);

        audit.Id.Should().Be(1);
        audit.UserId.Should().Be(100);
        audit.LoggedAt.Should().Be(new DateTime(2023, 1, 1));
        audit.Action.Should().Be(AuditAction.Deleted);

        var fieldChange = audit.Changes!.First().Change;
        fieldChange.FieldName.Should().Be(UserField.Forename);
        fieldChange.Before.Should().Be("Jonty");
        fieldChange.After.Should().Be("Jon");
    }

    [Fact]
    public void MapToDomainAuditChange_ShouldConverAuditChangeEntity()
    {
        var auditChangeEntity = new UserAuditChangeEntity
        {
            Id = 1,
            AuditId = 1,
            Field = "Forename",
            Before = "Jonty",
            After = "Jon"
        };

        var auditChange = UserAuditMapper.ToDomainAuditChange(auditChangeEntity);
        auditChange.AuditId.Should().Be(1);
        auditChange.Change.FieldName.Should().Be(UserField.Forename);
        auditChange.Change.Before.Should().Be("Jonty");
        auditChange.Change.After.Should().Be("Jon");
    }


}
