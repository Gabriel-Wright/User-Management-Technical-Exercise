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

public class DeleteUserFormTests : TestContext
{
    private readonly Mock<IUserApiService> _mockUserService;
    public DeleteUserFormTests()
    {
        _mockUserService = new Mock<IUserApiService>();
        Services.AddSingleton(_mockUserService.Object);
    }

    [Fact]
    public void DeleteUserForm_WhenNotVisible_RendersNothing()
    {
        var user = CreateTestUser();

        var cut = RenderComponent<DeleteUserForm>(parameters => parameters
            .Add(p => p.IsVisible, false)
            .Add(p => p.User, user));

        cut.Markup.Should().BeEmpty();
    }

    [Fact]
    public void DeleteUserForm_WhenVisible_DisplaysConfirmation()
    {
        var user = CreateTestUser();

        var cut = RenderComponent<DeleteUserForm>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.User, user));

        cut.Markup.Should().Contain("Are you sure you want to delete");
        cut.Markup.Should().Contain("John Doe");
    }

    [Fact]
    public async Task DeleteUserForm_WhenDeleteClicked_CallsApiService()
    {
        var user = CreateTestUser();
        var onDeletedCalled = false;

        _mockUserService
            .Setup(s => s.SoftDeleteUserAsync(user.Id))
            .Returns(Task.CompletedTask);

        var cut = RenderComponent<DeleteUserForm>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.User, user)
            .Add(p => p.OnDeleted, async (UserDto u) => { onDeletedCalled = true; await Task.CompletedTask; })
        );

        await cut.Find("button.btn-danger").ClickAsync(new());
        _mockUserService.Verify(s => s.SoftDeleteUserAsync(user.Id), Times.Once);
        onDeletedCalled.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserForm_WhenDeleteFails404_ShowsErrorMessage()
    {
        var user = CreateTestUser();

        _mockUserService
            .Setup(s => s.SoftDeleteUserAsync(user.Id))
            .ThrowsAsync(new UserApiException("Not found", 404));

        var cut = RenderComponent<DeleteUserForm>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.User, user));

        await cut.Find("button.btn-danger").ClickAsync(new());

        cut.Markup.Should().Contain("This user has already been deleted or no longer exists");
    }

    [Fact]
    public void DeleteUserForm_WhenCloseClicked_InvokesOnClose()
    {
        var user = CreateTestUser();
        var onCloseCalled = false;

        var cut = RenderComponent<DeleteUserForm>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.User, user)
            .Add(p => p.OnClose, () => { onCloseCalled = true; }));

        cut.Find("button.btn-secondary").Click();

        onCloseCalled.Should().BeTrue();
    }

    private static UserDto CreateTestUser()
    {
        return new UserDto
        {
            Id = 1,
            Forename = "John",
            Surname = "Doe",
            Email = "john@example.com",
            Role = 0,
            IsActive = true,
            BirthDate = new DateTime(1990, 1, 1)
        };
    }
}