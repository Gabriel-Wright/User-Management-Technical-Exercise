using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Services.Domain;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web;
using UserManagement.Web.Dtos;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    private readonly Mock<IUserService> _mockService;
    private readonly UsersController _controller;

    public UserControllerTests()
    {
        _mockService = new Mock<IUserService>();
        _controller = new UsersController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_WhenUsersExist_ReturnsOkWithUsers()
    {
        var users = new List<User>
        {
            new User { Id = 1, Forename = "John", Surname = "Doe", Email = "john@example.com" }
        };
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(users);

        var result = await _controller.GetAll();

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        var dtos = ok!.Value as IEnumerable<UserDto>;
        dtos.Should().HaveCount(1);
        dtos!.First().Forename.Should().Be("John");

        _mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoUsers_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<User>());

        var result = await _controller.GetAll();

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUsersById_WhenUserExists_ReturnsOk()
    {
        var user = new User { Id = 1, Forename = "Jane", Surname = "Doe", Email = "jane@example.com" };
        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _controller.GetUsersById(1);

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        var dto = ok!.Value as UserDto;
        dto!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetUsersById_WhenUserDoesNotExist_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((User?)null);

        var result = await _controller.GetUsersById(1);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUsersById_WhenIdInvalid_ReturnsBadRequest()
    {
        var result = await _controller.GetUsersById(-1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetUsersByStatus_WhenActiveUsersExist_ReturnsOkWithActiveUsers()
    {
        var users = new List<User>
        {
            new() { Id = 1, Forename = "John", Surname = "Doe", Email = "john@example.com", IsActive = true, BirthDate = DateTime.Now }
        };
        _mockService.Setup(s => s.FilterByActiveAsync(true)).ReturnsAsync(users);

        var result = await _controller.GetUsersByStatus(true);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = okResult.Value as IEnumerable<UserDto>;
        dtos.Should().HaveCount(1);
        dtos!.All(u => u.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetUsersByStatus_WhenNoActiveUsers_ReturnsNotFound()
    {
        _mockService.Setup(s => s.FilterByActiveAsync(true)).ReturnsAsync(new List<User>());

        var result = await _controller.GetUsersByStatus(true);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUsersByName_WhenUsersFound_ReturnsOk()
    {
        var users = new List<User>
        {
            new() { Id = 1, Forename = "John", Surname = "Doe", Email = "john@example.com", IsActive = true, BirthDate = DateTime.Now }
        };
        _mockService.Setup(s => s.GetByNameAsync("John", "Doe")).ReturnsAsync(users);

        var result = await _controller.GetUsersByName("John", "Doe");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = okResult.Value as IEnumerable<UserDto>;
        dtos.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUsersByName_WhenNoUsersFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetByNameAsync("NonExistent", "User")).ReturnsAsync(new List<User>());

        var result = await _controller.GetUsersByName("NonExistent", "User");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUsersByName_WhenForenameEmpty_ReturnsBadRequest()
    {
        var result = await _controller.GetUsersByName("", "Doe");

        //If we fail with bad request - we should NOT call the service layer.
        result.Should().BeOfType<BadRequestObjectResult>();

        _mockService.Verify(s => s.GetByNameAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetUsersByQuery_WhenUsersExist_ReturnsOkWithUsers()
    {
        var queryDto = new UserQueryDto
        {
            Page = 1,
            PageSize = 2,
            SortBy = "Forename",
            SortDescending = false,
            IsActive = true,
            SearchTerm = "John"
        };

        var users = new List<User>
    {
        new() { Id = 1, Forename = "John", Surname = "Doe", Email = "john@example.com", IsActive = true }
    };

        _mockService.Setup(s => s.GetUsersAsync(It.IsAny<UserQuery>()))
            .ReturnsAsync((users, users.Count));

        var result = await _controller.GetUsersByQuery(queryDto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value as PagedResult<UserDto>;
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().HaveCount(1);
        pagedResult.Items.First().Forename.Should().Be("John");

        _mockService.Verify(s => s.GetUsersAsync(It.Is<UserQuery>(q =>
            q.Page == queryDto.Page &&
            q.PageSize == queryDto.PageSize &&
            q.SortBy == queryDto.SortBy &&
            q.SortDescending == queryDto.SortDescending &&
            q.IsActive == queryDto.IsActive &&
            q.SearchTerm == queryDto.SearchTerm
        )), Times.Once);
    }

    [Fact]
    public async Task GetUsersByQuery_WhenNoUsersFound_ReturnsNotFound()
    {
        var queryDto = new UserQueryDto
        {
            Page = 1,
            PageSize = 5,
        };

        _mockService.Setup(s => s.GetUsersAsync(It.IsAny<UserQuery>()))
            .ReturnsAsync((new List<User>(), 0));

        var result = await _controller.GetUsersByQuery(queryDto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value as PagedResult<UserDto>;
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().BeEmpty();
        pagedResult.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetUsersByQuery_WhenSortByIsNull_DefaultsToId()
    {
        var queryDto = new UserQueryDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = null
        };

        var users = new List<User>
    {
        new() { Id = 1, Forename = "Alice", Surname = "Smith", Email = "alice@example.com" }
    };

        _mockService.Setup(s => s.GetUsersAsync(It.IsAny<UserQuery>()))
            .ReturnsAsync((users, users.Count));

        var result = await _controller.GetUsersByQuery(queryDto);

        _mockService.Verify(s => s.GetUsersAsync(It.Is<UserQuery>(q => q.SortBy == "Id")), Times.Once);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value as PagedResult<UserDto>;
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().HaveCount(1);
        pagedResult.Items.First().Forename.Should().Be("Alice");
    }

    public async Task AddUser_ValidDto_ReturnsCreatedAtAction()
    {
        var createDto = new UserCreateDto
        {
            Forename = "Mr",
            Surname = "Dto",
            Email = "valid@example.com",
            IsActive = true
        };

        //This is the expected user we expect to get back from Service 
        var returnedUser = new User
        {
            Id = 1,
            Forename = createDto.Forename,
            Surname = createDto.Surname,
            Email = createDto.Email,
            IsActive = createDto.IsActive
        };

        //But since we aren't testing user service in this unit -> we mock here
        _mockService.Setup(s => s.AddUserAsync(It.IsAny<User>()))
            .ReturnsAsync(returnedUser);

        var result = await _controller.AddUser(createDto);

        var created = result as CreatedAtActionResult;
        created.Should().NotBeNull();

        var dto = created!.Value as UserDto;
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(1);
        dto.Forename.Should().Be(createDto.Forename);
        dto.Surname.Should().Be(createDto.Surname);
        dto.Email.Should().Be(createDto.Email);

        //have to verify we saved
        _mockService.Verify(s => s.AddUserAsync(It.IsAny<User>()), Times.Once);
        _mockService.Verify(s => s.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task AddUser_WhenServiceThrowsInvalidOperation_ShouldPropagate()
    {
        var createDto = new UserCreateDto
        {
            Forename = "Bob",
            Surname = "Johnson",
            Email = "existing@example.com",
            IsActive = true,
            BirthDate = DateTime.Now
        };

        //we are not testing the checking of email already exists here - we have unit tested this in service layer.
        _mockService.Setup(s => s.AddUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

        Func<Task> act = async () => await _controller.AddUser(createDto);

        //will be caught by middle man
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already exists");
    }

    [Fact]
    public async Task UpdateUserPut_WithValidData_ReturnsOk()
    {
        // Arrange
        var dto = new UserDto
        {
            Id = 1,
            Forename = "Updated",
            Surname = "User",
            Email = "updated@example.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = DateTime.Now
        };

        //We expect this user to be created
        var updatedUser = new User
        {
            Id = dto.Id,
            Forename = dto.Forename,
            Surname = dto.Surname,
            Email = dto.Email,
            Role = dto.Role,
            IsActive = dto.IsActive,
            BirthDate = dto.BirthDate
        };

        //mock the expected user as a result - not testing service layer here
        _mockService.Setup(s => s.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(updatedUser);

        var result = await _controller.UpdateUserPut(1, dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var resultDto = okResult.Value as UserDto;
        resultDto.Should().NotBeNull();
        resultDto!.Forename.Should().Be("Updated");

        _mockService.Verify(s => s.UpdateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserPut_WhenIdMismatch_ReturnsBadRequest()
    {
        var dto = new UserDto
        {
            Id = 2,
            Forename = "Test",
            Surname = "User",
            Email = "test@example.com",
            IsActive = true,
            BirthDate = DateTime.Now
        };

        var result = await _controller.UpdateUserPut(1, dto);

        result.Should().BeOfType<BadRequestObjectResult>();
        _mockService.Verify(s => s.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserPut_WhenUserNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new UserDto
        {
            Id = 666,
            Forename = "Mr",
            Surname = "Devil",
            Email = "lucifier@askjeeves.com",
            IsActive = true,
            BirthDate = DateTime.Now
        };

        _mockService.Setup(s => s.UpdateUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new KeyNotFoundException("Mismatched IDs for update"));

        // Act
        Func<Task> act = async () => await _controller.UpdateUserPut(666, dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateUserPatch_WithValidPatch_ReturnsOk()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Forename = "Johnny",
            Surname = "IsHere",
            Email = "john@stanleyK.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = DateTime.Now
        };

        var patchDto = new UserPatchDto
        {
            Forename = "Johnathan"
        };

        //expected updated user if we were connected with service layer
        var updatedUser = new User
        {
            Id = 1,
            Forename = "Johnathan",
            Surname = "Doe",
            Email = "john@example.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = DateTime.Now
        };

        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(existingUser);
        _mockService.Setup(s => s.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(updatedUser);

        var result = await _controller.UpdateUserPatch(1, patchDto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value as UserDto;
        dto.Should().NotBeNull();
        dto!.Forename.Should().Be("Johnathan");
    }

    [Fact]
    public async Task UpdateUserPatch_WhenUserNotFound_ReturnsNotFound()
    {
        var patchDto = new UserPatchDto { Forename = "Test" };
        _mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((User?)null);

        var result = await _controller.UpdateUserPatch(999, patchDto);

        result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(s => s.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUser_WhenUserExists_ReturnsNoContent()
    {
        _mockService.Setup(s => s.DeleteUserAsync(1)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteUser(1);

        result.Should().BeOfType<NoContentResult>();
        _mockService.Verify(s => s.DeleteUserAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_WhenUserNotFound_ThrowsKeyNotFoundException()
    {
        _mockService.Setup(s => s.DeleteUserAsync(999))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        Func<Task> act = async () => await _controller.DeleteUser(999);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SoftDeleteUser_WhenUserExists_ReturnsNoContent()
    {
        _mockService.Setup(s => s.SoftDeleteUserAsync(1)).Returns(Task.CompletedTask);

        var result = await _controller.SoftDeleteUser(1);

        result.Should().BeOfType<NoContentResult>();
        _mockService.Verify(s => s.SoftDeleteUserAsync(1), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteUser_WhenIdInvalid_ReturnsBadRequest()
    {
        var result = await _controller.SoftDeleteUser(0);

        result.Should().BeOfType<BadRequestObjectResult>();
        _mockService.Verify(s => s.SoftDeleteUserAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task SoftDeleteUser_WhenUserNotFound_ThrowsKeyNotFoundException()
    {
        _mockService.Setup(s => s.SoftDeleteUserAsync(666))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        Func<Task> act = async () => await _controller.SoftDeleteUser(666);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");

        _mockService.Verify(s => s.SoftDeleteUserAsync(666), Times.Once);//reaches services but cant find user
    }

}
