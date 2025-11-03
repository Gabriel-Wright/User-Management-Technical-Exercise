using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement.Data.Tests;

public class DataContextTests
{
    [Fact]
    public async Task GetAll_WhenNewUserEntityAdded_MustIncludeNewUserEntity()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var context = CreateContext();

        var entity = new UserEntity
        {
            Forename = "Brand New",
            Surname = "User",
            Email = "brandnewuser@example.com",
            UserRole = "User",
        };
        await context.CreateAsync(entity);

        // Act: Invokes the method under test with the arranged parameters.
        var result = context.GetAll<UserEntity>();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result
            .Should().Contain(s => s.Email == entity.Email)
            .Which.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public async Task Update_WhenUserEntityChanged_ShouldPersistChanges()
    {
        var context = CreateContext();
        var user = new UserEntity { Forename = "Test", Surname = "User", Email = "test@user.com" };
        await context.CreateAsync(user);

        user.Surname = "Updated";
        context.Update(user);

        var result = context.GetAll<UserEntity>().First(u => u.Email == "test@user.com");
        result.Surname.Should().Be("Updated");
    }

    [Fact]
    public async Task GetAll_WhenDeleted_MustNotIncludeDeletedUserEntity()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var context = CreateContext();

        var entity = new UserEntity
        {
            Forename = "Brand New",
            Surname = "User",
            Email = "brandnewuser@example.com",
            UserRole = "User",
        };
        await context.CreateAsync(entity);

        var result = context.GetAll<UserEntity>();

        result
            .Should().Contain(s => s.Email == entity.Email)
            .Which.Should().BeEquivalentTo(entity);


        await context.DeleteAsync(entity);


        result = context.GetAll<UserEntity>();

        ////Should be deleted now
        result.Should().NotContain(s => s.Email == entity.Email);
    }

    [Fact]
    public async Task AuditEntity_ShouldBePersisted()
    {
        var context = CreateContext();
        var audit = new AuditEntity { EntityId = 1, AuditAction = "Created" };

        await context.CreateAsync(audit);

        var result = await context.GetAll<AuditEntity>().FirstAsync();
        result.EntityId.Should().Be(1);
        result.AuditAction.Should().Be("Created");
    }

    [Fact]
    public async Task AuditEntity_WhenDeleted_MustNotIncludeEntity()
    {
        var context = CreateContext();
        var audit = new AuditEntity { EntityId = 1, AuditAction = "Created" };

        await context.CreateAsync(audit);

        var result = await context.GetAll<AuditEntity>().FirstAsync();
        result.EntityId.Should().Be(1);
        result.AuditAction.Should().Be("Created");

        await context.DeleteAsync(audit);

        var audits = context.GetAll<AuditEntity>();
        audits.Should().NotContain(a => a.EntityId == 1);
    }


    //New DB per test to ensure isolation
    private DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }
}
