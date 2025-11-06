using System;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserManagement.UI.Components;
using UserManagement.UI.Dtos;
using UserManagement.UI.Exceptions;

namespace UserManagement.UI.Tests.Components;

public class CreateUserFormTests : TestContext
{
    private readonly Mock<IUserApiService> _mockUserService;

    public CreateUserFormTests()
    {
        _mockUserService = new Mock<IUserApiService>();
        Services.AddSingleton(_mockUserService.Object);
    }

    [Fact]
    public void CreateUserForm_WhenNotVisible_RendersNothing()
    {
        var cut = RenderComponent<CreateUserForm>(parameters => parameters
            .Add(p => p.IsVisible, false));

        cut.Markup.Should().BeEmpty();
    }

    [Fact]
    public void CreateUserForm_WhenVisible_ShowsEmptyForm()
    {
        var cut = RenderComponent<CreateUserForm>(parameters => parameters
            .Add(p => p.IsVisible, true));

        cut.Markup.Should().Contain("Create User");
        var inputs = cut.FindAll("input[class='form-control']");
        inputs.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateUserForm_WhenValidSubmit_CallsApiService()
    {
        var newUser = new UserDto
        {
            Id = 1,
            Forename = "Alice",
            Surname = "Smith",
            Email = "alice@example.com",
            Role = 0,
            IsActive = true,
            BirthDate = new DateTime(1995, 5, 15)
        };
        var onCreatedCalled = false;

        _mockUserService
            .Setup(s => s.CreateUserAsync(It.IsAny<UserCreateDto>()))
            .ReturnsAsync(newUser);

        var cut = RenderComponent<CreateUserForm>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnCreated, (UserDto u) => { onCreatedCalled = true; }));

        cut.Find("#forename").Change("Alice");
        cut.Find("#surname").Change("Smith");
        cut.Find("#email").Change("alice@example.com");
        cut.Find("#role").Change("0");
        cut.Find("#isActive").Change(true);
        cut.Find("#birthday").Change("1995-05-15");

        await cut.Find("form").SubmitAsync();

        _mockUserService.Verify(s => s.CreateUserAsync(
            It.Is<UserCreateDto>(dto =>
                dto.Forename == "Alice" &&
                dto.Surname == "Smith" &&
                dto.Email == "alice@example.com")
        ), Times.Once);
        onCreatedCalled.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUserForm_WhenEmailExists_ShowsConflictError()
    {
        _mockUserService
            .Setup(s => s.CreateUserAsync(It.IsAny<UserCreateDto>()))
            .ThrowsAsync(new UserApiException("Conflict", 409));

        var cut = RenderComponent<CreateUserForm>(parameters => parameters
            .Add(p => p.IsVisible, true));

        cut.Find("#forename").Change("Alice");
        cut.Find("#surname").Change("Smith");
        cut.Find("#email").Change("alice@example.com");
        cut.Find("#role").Change("0");
        cut.Find("#isActive").Change(true);
        cut.Find("#birthday").Change("1995-05-15");

        await cut.Find("form").SubmitAsync();

        cut.Markup.Should().Contain("A user with this email already exists");
    }

    [Fact]
    public void CreateUserForm_WhenCloseClicked_InvokesOnClose()
    {
        var onCloseCalled = false;

        var cut = RenderComponent<CreateUserForm>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnClose, () => { onCloseCalled = true; }));

        cut.Find("button.btn-secondary").Click();

        onCloseCalled.Should().BeTrue();
    }
}