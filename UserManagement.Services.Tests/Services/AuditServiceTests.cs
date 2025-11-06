using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain;
using UserManagement.Services.Domain.Implementations;
using UserMangement.Services.Events;

public class AuditServiceTests
{
    [Fact]
    public async Task CreateUserCreatedAuditAsync_ShouldCreateAuditAndChanges()
    {
        var mockContext = new Mock<IDataContext>();

        var createdAudits = new List<UserAuditEntity>();
        var createdChanges = new List<UserAuditChangeEntity>();

        mockContext.Setup(c => c.CreateAsync(It.IsAny<UserAuditEntity>()))
                   .Returns<UserAuditEntity>(entity =>
                   {
                       entity.Id = 1;
                       createdAudits.Add(entity);
                       return Task.CompletedTask;
                   });

        mockContext.Setup(c => c.CreateAsync(It.IsAny<UserAuditChangeEntity>()))
                   .Returns<UserAuditChangeEntity>(change =>
                   {
                       createdChanges.Add(change);
                       return Task.CompletedTask;
                   });

        mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        var auditService = new AuditService(mockContext.Object);

        var user = new User
        {
            Forename = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = new DateTime(1990, 1, 1)
        };

        await auditService.CreateUserCreatedAuditAsync(123, user);

        createdAudits.Should().ContainSingle();
        var audit = createdAudits[0];
        audit.UserEntityId.Should().Be(123);
        audit.AuditAction.Should().Be("Created");

        createdChanges.Should().HaveCount(6);
        createdChanges.Should().Contain(c => c.Field == "Forename" && c.After == "John");
        createdChanges.Should().Contain(c => c.Field == "Surname" && c.After == "Doe");
        createdChanges.Should().Contain(c => c.Field == "Email" && c.After == "john.doe@example.com");
        createdChanges.Should().Contain(c => c.Field == "Role" && c.After == "User");
        createdChanges.Should().Contain(c => c.Field == "IsActive" && c.After == "True");
        createdChanges.Should().Contain(c => c.Field == "BirthDate" && c.After == "1990-01-01");

        mockContext.Verify(c => c.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task AuditService_Handle_ShouldCreateAuditRecords()
    {
        var context = CreateContext();
        var auditService = new AuditService(context);

        var user = new User
        {
            Forename = "Bus",
            Surname = "Man",
            Email = "ManBus@Drivers.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = DateTime.Today.AddYears(-25)
        };

        var evt = new UserCreatedEvent { UserId = 1, User = user };

        await auditService.Handle(evt);

        var audit = await context.GetAll<UserAuditEntity>().FirstOrDefaultAsync();
        audit.Should().NotBeNull();
        audit.AuditAction.Should().Be("Created");

        var changes = await context.GetAll<UserAuditChangeEntity>().ToListAsync();
        changes.Should().HaveCount(6);
        changes.Select(c => c.Field).Should().Contain(new[] { "Forename", "Surname", "Email", "Role", "IsActive", "BirthDate" });
    }

    private DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDataContext(options);
    }

    private class TestDataContext : DataContext
    {
        public TestDataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Filter still applied
            modelBuilder.Entity<UserEntity>().HasQueryFilter(u => !u.Deleted);

            //No seed data
        }
    }


}
