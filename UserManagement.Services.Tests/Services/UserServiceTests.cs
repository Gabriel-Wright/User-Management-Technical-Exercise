using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain;
using UserManagement.Services.Domain.Implementations;
using UserManagement.Services.Events;
using UserMangement.Services.Events;

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
        var service = CreateUserService(context);
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
    public async Task GetUsersAsync_NoMatches_ShouldReturnEmptyList()
    {
        var context = CreateContext();
        var service = CreateUserService(context);

        await AddFiveTestUsers(context);
        await service.SaveAsync();

        var query = new UserQuery { SearchTerm = "nonexistent" };
        var (users, totalCount) = await service.GetUsersAsync(query);

        totalCount.Should().Be(0);
        users.Should().BeEmpty();
    }
    [Fact]
    public async Task GetUsersAsync_FilterSearchByTerm_ShouldFindResults()
    {
        var context = CreateContext();
        var service = CreateUserService(context);

        await AddFiveTestUsers(context);
        await service.SaveAsync();

        var query = new UserQuery
        {
            SearchTerm = "gryffindor",
            SortDescending = true,
        };

        var (users, totalCount) = await service.GetUsersAsync(query);

        totalCount.Should().Be(3); //3 total users and not separating by page
        users.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetUsersAsync_FilterSearchByTermAndActive_ShouldFindOneResults()
    {
        var context = CreateContext();
        var service = CreateUserService(context);

        await AddFiveTestUsers(context);
        await service.SaveAsync();

        var query = new UserQuery
        {
            IsActive = false,
            SearchTerm = "gryffindor",
            SortDescending = true,
        };

        var (users, totalCount) = await service.GetUsersAsync(query);

        totalCount.Should().Be(1); //3 total users and not separating by page
        users.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUsersAsync_Sorting_ShouldReturnUsersInCorrectSurnameDescOrder()
    {
        //For reference: If sorted by Surname Desc order
        //Ronald WEASLEY <- First
        //Lord Voldemort
        //Harry Potter
        //Hagrid IDK
        //Hermione Granger <- Last
        var context = CreateContext();
        var service = CreateUserService(context);

        await AddFiveTestUsers(context);
        await service.SaveAsync();

        var query = new UserQuery { SortBy = "Surname", SortDescending = true };
        var (users, totalCount) = await service.GetUsersAsync(query);

        users.First().Forename.Should().Be("Ron");
        users.Last().Forename.Should().Be("Hermione");

    }
    [Fact]
    public async Task GetUsersAsync_InvalidPageOrPageSize_ShouldUseDefaults()
    {
        var context = CreateContext();
        var service = CreateUserService(context);

        await AddFiveTestUsers(context);
        await service.SaveAsync();

        var query = new UserQuery { Page = -1, PageSize = 0 };
        var (users, totalCount) = await service.GetUsersAsync(query);

        totalCount.Should().Be(5);
        users.Should().HaveCount(5); // default pageSize = 10, so all 5 returned
    }
    [Fact]
    public async Task GetAllAsync_WhenNoUsers_ShouldReturnEmptyList()
    {
        var context = CreateContext();
        var service = CreateUserService(context);
        var result = await service.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsersAsync_WithSmallPage_ShouldReturnTwoUsers()
    {
        var context = CreateContext();
        var service = CreateUserService(context);

        await AddFiveTestUsers(context);
        await service.SaveAsync();

        var query = new UserQuery
        {
            Page = 1,
            PageSize = 2
        };

        var (users, totalCount) = await service.GetUsersAsync(query);

        totalCount.Should().Be(5); // 5 total users but only 2 in the page
        users.Should().HaveCount(2);
        users.Should().Contain(u => u.Email == "gryffindorOne@gmail.com");
        users.Should().Contain(u => u.Email == "gryffindorTwo@yahoo.com");
    }

    [Fact]
    public async Task GetUsersAsync_WithSmallPageThirdPage_ShouldReturnOneUsers()
    {
        var context = CreateContext();
        var service = CreateUserService(context);

        await AddFiveTestUsers(context);
        await service.SaveAsync();

        var query = new UserQuery
        {
            Page = 3,
            PageSize = 2
        };

        var (users, totalCount) = await service.GetUsersAsync(query);

        totalCount.Should().Be(5); // 5 total users but only 2 in the page
        users.Should().HaveCount(1);
        users.Should().Contain(u => u.Email == "groundskeeper@currys.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        var context = CreateContext();
        var service = CreateUserService(context);

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
        var service = CreateUserService(context);

        var result = await service.GetByIdAsync(1);


        result.Should().BeNull();
    }

    [Fact]
    public async Task AddUserAsync_WithValidUser_ShouldAddUser()
    {
        var context = CreateContext();
        var service = CreateUserService(context);

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
        result.Id.Should().Be(1);
        result.Email.Should().Be("billy@ballet.com");

        var usersInDb = await context.Users!.ToListAsync();
        usersInDb.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddUserAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        var context = CreateContext();
        var service = CreateUserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        var duplicateUser = new User
        {
            Forename = "Mr",
            Surname = "Messy",
            Email = "messy@gmail.com",
            IsActive = true,
            BirthDate = DateTime.Today.AddYears(-30)
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
        var service = CreateUserService(context);

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
        var service = CreateUserService(context);

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
        var service = CreateUserService(context);

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
        var service = CreateUserService(context);

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
        var service = CreateUserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        var existingUser = await service.GetByIdAsync(1);
        existingUser!.Email = "invalid-email";  // Invalid

        Func<Task> act = async () => await service.UpdateUserAsync(existingUser);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SoftDeleteUserAsync_WhenUserExists_ShouldMarkDeletedAndHideFromQueries()
    {
        // Arrange
        var context = CreateContext();
        var service = CreateUserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        //Get test users
        var user = (await service.GetAllAsync()).First();

        //Soft delete them
        await service.SoftDeleteUserAsync(user.Id);
        await service.SaveAsync();

        //User can't be found
        var users = await service.GetAllAsync();
        users.Should().NotContain(u => u.Id == user.Id);

        //Think this is the best way to test it?
        var entityInDb = await context.Users!.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == user.Id);
        entityInDb.Should().NotBeNull();
        entityInDb!.Deleted.Should().BeTrue();
    }

    [Fact]
    public async Task SoftDeleteUserAsync_WhenUserDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var context = CreateContext();
        var service = CreateUserService(context);

        await AddTestUsers(context);
        await service.SaveAsync();

        Func<Task> act = async () => await service.SoftDeleteUserAsync(999);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    //Birthday attribute validation
    [Fact]
    public async Task AddUserAsync_WithBirthDateTooYoung_ShouldThrowValidationException()
    {
        var context = CreateContext();
        var service = CreateUserService(context);

        var newUser = new User
        {
            Forename = "Bart",
            Surname = "Simpson",
            Email = "cowman@yahoo.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = DateTime.Today.AddYears(-17)
        };

        Func<Task> act = async () => await service.AddUserAsync(newUser);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("User must be at least 18 years old.");
    }

    [Fact]
    public async Task AddUserAsync_WithBirthDateTooOld_ShouldThrowValidationException()
    {
        var context = CreateContext();
        var service = CreateUserService(context);

        var newUser = new User
        {
            Forename = "Abe",
            Surname = "Simpson",
            Email = "abe@example.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = DateTime.Today.AddYears(-121)
        };

        Func<Task> act = async () => await service.AddUserAsync(newUser);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("User cannot be older than 120 years.");
    }

    //Checking create method does fire off expected event
    [Fact]
    public async Task AddUserAsync_ShouldPublishUserCreatedEvent()
    {
        var context = CreateContext();
        var mockEventBus = new Mock<IEventBus>();
        var service = new UserService(context, mockEventBus.Object);

        var newUser = new User
        {
            Forename = "Event",
            Surname = "Man",
            Email = "MrEvent@Yahoo.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = DateTime.Today.AddYears(-25)
        };

        var result = await service.AddUserAsync(newUser);

        mockEventBus.Verify(
            bus => bus.PublishAsync(It.Is<UserCreatedEvent>(evt =>
                evt.User.Id == newUser.Id &&
                evt.User.Email == newUser.Email &&
                evt.User.Forename == newUser.Forename
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldPublishUserUpdatedEvent()
    {
        var context = CreateContext();
        var mockEventBus = new Mock<IEventBus>();
        var service = new UserService(context, mockEventBus.Object);

        var existingUser = new User
        {
            Forename = "Event",
            Surname = "Man",
            Email = "MrEvent@Yahoo.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = DateTime.Today.AddYears(-25)
        };

        var addedUser = await service.AddUserAsync(existingUser);

        addedUser.Email = "updated@domain.com";
        addedUser.IsActive = false;

        var updatedUser = await service.UpdateUserAsync(addedUser);

        mockEventBus.Verify(
            bus => bus.PublishAsync(It.Is<UserUpdatedEvent>(evt =>
                evt.UserId == updatedUser.Id &&
                evt.NewUser.Email == updatedUser.Email &&
                evt.NewUser.Forename == updatedUser.Forename
            )),
            Times.Once
        );
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
            //Filter still applied
            modelBuilder.Entity<UserEntity>().HasQueryFilter(u => !u.Deleted);

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
            IsActive = true,
            BirthDate = DateTime.Today.AddYears(-30)
        });

        await context.CreateAsync(new UserEntity
        {
            Forename = "Mrs",
            Surname = "Tickle",
            Email = "tickle@yahoo.com",
            IsActive = false,
            BirthDate = DateTime.Today.AddYears(-40)
        });
    }

    private static async Task AddFiveTestUsers(DataContext context)
    {
        await context.CreateAsync(new UserEntity
        {
            Forename = "Harry",
            Surname = "Potter",
            Email = "gryffindorOne@gmail.com",
            IsActive = true
        });

        await context.CreateAsync(new UserEntity
        {
            Forename = "Ron",
            Surname = "Weasley",
            Email = "gryffindorTwo@yahoo.com",
            IsActive = false
        });

        await context.CreateAsync(new UserEntity
        {
            Forename = "Hermione",
            Surname = "Granger",
            Email = "gryffindorThree@aol.com",
            IsActive = true
        });

        await context.CreateAsync(new UserEntity
        {
            Forename = "Lord",
            Surname = "Voldemort",
            Email = "slytherine@askjeeves.com",
            IsActive = false
        });

        await context.CreateAsync(new UserEntity
        {
            Forename = "Hagrid",
            Surname = "Idk",
            Email = "groundskeeper@currys.com",
            IsActive = false
        });
    }

    private UserService CreateUserService(DataContext context)
    {
        var mockEventBus = new Mock<IEventBus>();
        return new UserService(context, mockEventBus.Object);
    }
}