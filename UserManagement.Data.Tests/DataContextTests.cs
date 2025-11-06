using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        await context.SaveChangesAsync();

        // Act: Invokes the method under test with the arranged parameters.
        var result = context.GetAll<UserEntity>();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result
            .Should().Contain(s => s.Email == entity.Email)
            .Which.Should().BeEquivalentTo(entity);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Update_WhenUserEntityChanged_ShouldPersistChanges()
    {
        var context = CreateContext();
        var user = new UserEntity { Forename = "Test", Surname = "User", Email = "test@user.com" };
        await context.CreateAsync(user);
        await context.SaveChangesAsync();


        user.Surname = "Updated";
        context.Update(user);
        await context.SaveChangesAsync();


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
        await context.SaveChangesAsync();

        var result = context.GetAll<UserEntity>();

        result.Should().ContainSingle(u => u.Email == entity.Email)
              .Which.Forename.Should().Be(entity.Forename);

        context.Delete(entity);
        await context.SaveChangesAsync();

        result = context.GetAll<UserEntity>();

        ////Should be deleted now
        result.Should().NotContain(s => s.Email == entity.Email);
    }

    [Fact]
    public async Task CreateAudit_WhenNewAuditAdded_ShouldBePersisted()
    {
        var context = CreateContext();
        var user = new UserEntity { Forename = "Test", Surname = "User", Email = "test@user.com" };
        await context.CreateAsync(user);
        await context.SaveChangesAsync();

        var audit = new UserAuditEntity
        {
            UserEntityId = user.Id,
            AuditAction = "Created"
        };

        await context.CreateAsync(audit);
        await context.SaveChangesAsync();

        var result = context.GetAll<UserAuditEntity>().FirstOrDefault();
        result.Should().NotBeNull();
        result!.UserEntityId.Should().Be(user.Id);
        result.AuditAction.Should().Be("Created");
        result.LoggedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AuditEntity_WhenDeleted_MustNotIncludeEntity()
    {
        var context = CreateContext();
        var user = new UserEntity { Forename = "Test", Surname = "User", Email = "test@user.com" };
        await context.CreateAsync(user);
        await context.SaveChangesAsync();

        var audit = new UserAuditEntity
        {
            UserEntityId = user.Id,
            AuditAction = "Created"
        };

        await context.CreateAsync(audit);
        await context.SaveChangesAsync();

        var result = await context.GetAll<UserAuditEntity>().ToListAsync();
        result.Should().ContainSingle(u => u.AuditAction == audit.AuditAction)
                  .Which.AuditAction.Should().Be("Created");


        context.Delete(audit);
        await context.SaveChangesAsync();
        var audits = context.GetAll<UserAuditEntity>();
        audits.Should().NotContain(a => a.Id == 1);
    }

    [Fact]
    public async Task CreateAuditChange_ShouldPersistAndLinkToAudit()
    {
        var context = CreateContext();
        var user = new UserEntity { Forename = "Change", Surname = "Test", Email = "change@test.com" };
        await context.CreateAsync(user);
        await context.SaveChangesAsync();

        var audit = new UserAuditEntity { UserEntityId = user.Id, AuditAction = "Updated" };
        await context.CreateAsync(audit);
        await context.SaveChangesAsync();

        var change = new UserAuditChangeEntity
        {
            AuditId = audit.Id,
            Field = "Surname",
            Before = "Test",
            After = "Updated"
        };
        await context.CreateAsync(change);
        await context.SaveChangesAsync();

        var loadedChange = context.GetAll<UserAuditChangeEntity>()
                                  .Include(c => c.Audit)
                                  .First();
        loadedChange.Audit.Should().NotBeNull();
        loadedChange.Audit.Id.Should().Be(audit.Id);
        loadedChange.Before.Should().Be("Test");
        loadedChange.After.Should().Be("Updated");
    }

    [Fact]
    public async Task AuditNavigationProperty_ShouldLinkToUserEntity()
    {
        var context = CreateContext();
        var user = new UserEntity { Forename = "Nav", Surname = "Test", Email = "nav@test.com" };
        await context.CreateAsync(user);
        await context.SaveChangesAsync();

        var audit = new UserAuditEntity
        {
            UserEntityId = user.Id,
            AuditAction = "Updated"
        };
        await context.CreateAsync(audit);
        await context.SaveChangesAsync();

        var loadedAudit = context.GetAll<UserAuditEntity>()
                                 .Include(a => a.UserEntity)
                                 .First();
        loadedAudit.UserEntity.Should().NotBeNull();
        loadedAudit.UserEntity.Email.Should().Be(user.Email);
    }

    //
    // TESTING SOFT DELETE
    //
    [Fact]
    public async Task SoftDelete_WhenUserEntityDeleted_ShouldBeFilteredFromQueries()
    {
        var context = CreateContext();
        var entity = new UserEntity
        {
            Forename = "Soft",
            Surname = "Delete",
            Email = "softdelete@example.com",
            UserRole = "User",
        };
        await context.CreateAsync(entity);
        await context.SaveChangesAsync();

        //Mark as deleted
        entity.Deleted = true;
        context.UpdateE(entity);
        await context.SaveChangesAsync();

        //With query filter default applied in DataContext - should hide it
        var users = context.GetAll<UserEntity>();
        users.Should().NotContain(u => u.Email == entity.Email);

        //Ignoring filters - should find entity
        var allUsers = context.Set<UserEntity>().IgnoreQueryFilters();
        allUsers.Should().ContainSingle(u => u.Email == entity.Email && u.Deleted);
    }



    //New DB per test to ensure isolation
    private DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDataContext(options);
    }
}

public class TestDataContext : DataContext
{
    public TestDataContext(DbContextOptions<DataContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Filter still applied
        modelBuilder.Entity<UserEntity>().HasQueryFilter(u => !u.Deleted);

        //No seed data
    }
}

