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
    public async Task AuditEntity_ShouldBePersisted()
    {
        var context = CreateContext();
        var audit = new AuditEntity { EntityId = 1, AuditAction = "Created" };

        await context.CreateAsync(audit);
        await context.SaveChangesAsync();

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
        await context.SaveChangesAsync();

        var result = await context.GetAll<AuditEntity>().ToListAsync();
        result.Should().ContainSingle(u => u.AuditAction == audit.AuditAction)
                  .Which.AuditAction.Should().Be("Created");


        context.Delete(audit);
        await context.SaveChangesAsync();
        var audits = context.GetAll<AuditEntity>();
        audits.Should().NotContain(a => a.EntityId == 1);
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
        //No seed data
    }
}

