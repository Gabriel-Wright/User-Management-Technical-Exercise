using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Services.Tests;

/// <summary>
/// In this test class - use CreateContext() and AddTestUsers() methods in each test where necessary.
/// I realise I could have a different setup - so these are automatically called at the start of each test,
/// but I find it easier to read if all the setup for a test is shown in that test.
/// </summary>
public class UserServiceTests
{

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        var context = CreateContext();
        var service = new UserService(context);
        var result = await service.GetAllAsync();

        result.Should().HaveCount(0);

        await AddTestUsers(context);
        await context.SaveChangesAsync();

        result = await service.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Email == "messy@gmail.com");
        result.Should().Contain(u => u.Email == "tickle@yahoo.com");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoUsers_ShouldReturnEmptyList()
    {
        var context = CreateContext();
        var service = new UserService(context);
        var result = await service.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FilterByActiveAsync_WhenActiveTrue_ShouldReturnOnlyActiveUsers()
    {
        var context = CreateContext();
        var service = new UserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        var result = await service.FilterByActiveAsync(true);

        result.Should().HaveCount(1);
        result.Single().Email.Should().Be("messy@gmail.com");
        result.All(u => u.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        var context = CreateContext();
        var service = new UserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        var result = await service.GetByIdAsync(1);

        result!.Id.Should().Be(1);
        result.Forename.Should().Be("Mr");
        result.Surname.Should().Be("Messy");
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        var context = CreateContext();
        var service = new UserService(context);

        var result = await service.GetByIdAsync(1);


        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_WhenUserExists_ShouldReturnMatchingUsers()
    {

        var context = CreateContext();
        var service = new UserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        var result = await service.GetByNameAsync("Mr", "Messy");

        // Assert
        result.Should().ContainSingle();
        result.Single().Email.Should().Be("messy@gmail.com");
    }

    [Fact]
    public async Task GetByNameAsync_ShouldBeCaseInsensitive()
    {
        var context = CreateContext();
        var service = new UserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        var result = await service.GetByNameAsync("MR", "MESSY");

        // Assert
        result.Should().ContainSingle();
        result.Single().Email.Should().Be("messy@gmail.com");

    }

    [Fact]
    public async Task GetByNameAsync_WhenForenameEmpty_ShouldThrowArgumentException()
    {
        var context = CreateContext();
        var service = new UserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        Func<Task> act = async () => await service.GetByNameAsync("", "Barney");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Forename*");
    }

    [Fact]
    public async Task GetByNameAsync_WhenNoMatch_ShouldReturnEmptyList()
    {
        var context = CreateContext();
        var service = new UserService(context);

        var result = await service.GetByNameAsync("Mrs", "Tumble");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddUserAsync_WithValidUser_ShouldAddUser()
    {
        var context = CreateContext();
        var service = new UserService(context);

        var newUser = new User
        {
            Forename = "Billy",
            Surname = "Elliot",
            Email = "billy@ballet.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = new DateTime(1992, 3, 10)
        };

        var result = await service.AddUserAsync(newUser);
        await service.SaveAsync();

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Email.Should().Be("billy@ballet.com");

        var usersInDb = await context.Users!.ToListAsync();
        usersInDb.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddUserAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        var context = CreateContext();
        var service = new UserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        var duplicateUser = new User
        {
            Forename = "Mr",
            Surname = "Messy",
            Email = "messy@gmail.com",
            IsActive = true
        };

        Func<Task> act = async () => await service.AddUserAsync(duplicateUser);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task AddUserAsync_WithInvalidEmail_ShouldThrowValidationException()
    {
        var context = CreateContext();
        var service = new UserService(context);

        // Arrange
        var invalidUser = new User
        {
            Forename = "i",
            Surname = "am",
            Email = "not-real", //bad email
            Role = UserRole.User,
            IsActive = true,
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        Func<Task> act = async () => await service.AddUserAsync(invalidUser);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task AddUserAsync_WithMissingForename_ShouldThrowValidationException()
    {
        var context = CreateContext();
        var service = new UserService(context);

        // Arrange
        var invalidUser = new User
        {
            Forename = "",  // Required
            Surname = "Bad",
            Email = "forename@user.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        Func<Task> act = async () => await service.AddUserAsync(invalidUser);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidChanges_ShouldUpdateUser()
    {
        var context = CreateContext();
        var service = new UserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        var existingUser = await service.GetByIdAsync(1);
        existingUser!.Forename = "NewForename";
        existingUser.Email = "new@email.com";

        // Act
        var result = await service.UpdateUserAsync(existingUser);
        await service.SaveAsync();

        // Assert
        result.Forename.Should().Be("NewForename");
        result.Email.Should().Be("new@email.com");

        result = await service.GetByIdAsync(1);

        var updatedUser = await service.GetByIdAsync(existingUser.Id);
        updatedUser!.Forename.Should().Be("NewForename");
        updatedUser.Email.Should().Be("new@email.com");
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var context = CreateContext();
        var service = new UserService(context);

        // Arrange
        var nonExistentUser = new User
        {
            Id = 999,
            Forename = "Cant",
            Surname = "find",
            Email = "invisible@man.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        Func<Task> act = async () => await service.UpdateUserAsync(nonExistentUser);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task UpdateUserAsync_WithInvalidData_ShouldThrowValidationException()
    {
        var context = CreateContext();
        var service = new UserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        var existingUser = await service.GetByIdAsync(1);
        existingUser!.Email = "invalid-email";  // Invalid

        Func<Task> act = async () => await service.UpdateUserAsync(existingUser);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_ShouldDeleteUser()
    {
        var context = CreateContext();
        var service = new UserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        var users = await service.GetAllAsync();
        users.Should().HaveCount(2);


        await service.DeleteUserAsync(1);
        await service.SaveAsync();

        users = await service.GetAllAsync();
        users.Should().HaveCount(1);
        users.Should().NotContain(u => u.Id == 1);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var context = CreateContext();
        var service = new UserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        Func<Task> act = async () => await service.DeleteUserAsync(999);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    ///   ====================
    ///   TEST SETUP FUNCTIONS
    ///   ====================

    /// <returns></returns>
    //New DB per test to ensure isolation
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
            //No seed data
        }
    }

    private static async Task AddTestUsers(DataContext context)
    {
        await context.CreateAsync(new UserEntity
        {
            Forename = "Mr",
            Surname = "Messy",
            Email = "messy@gmail.com",
            IsActive = true
        });

        await context.CreateAsync(new UserEntity
        {
            Forename = "Mrs",
            Surname = "Tickle",
            Email = "tickle@yahoo.com",
            IsActive = false
        });
    }
}