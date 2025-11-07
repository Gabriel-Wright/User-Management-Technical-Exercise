using Bunit;
using FluentAssertions;
using UserManagement.UI.Components;
using UserManagement.UI.Dtos;

namespace UserManagement.UI.Tests.Components;

public class ViewUserFormTests : TestContext
{
    [Fact]
    public void ViewUserForm_DisplaysUserDetails()
    {
        var user = CreateTestUser();

        var cut = RenderComponent<ViewUserForm>(parameters => parameters
            .Add(p => p.User, user));

        cut.Markup.Should().Contain("John");
        cut.Markup.Should().Contain("Doe");
        cut.Markup.Should().Contain("john@example.com");
        cut.Markup.Should().Contain("User Details");
    }

    [Fact]
    public void ViewUserForm_WhenActiveUser_ShowsActiveStatus()
    {
        var user = CreateTestUser(isActive: true);

        var cut = RenderComponent<ViewUserForm>(parameters => parameters
            .Add(p => p.User, user));

        cut.Markup.Should().Contain("Active");
        cut.Markup.Should().Contain("bg-success");
    }

    [Fact]
    public void ViewUserForm_WhenInactiveUser_ShowsInactiveStatus()
    {
        var user = CreateTestUser(isActive: false);

        var cut = RenderComponent<ViewUserForm>(parameters => parameters
            .Add(p => p.User, user));

        cut.Markup.Should().Contain("Inactive");
        cut.Markup.Should().Contain("bg-secondary");
    }

    [Fact]
    public void ViewUserForm_WhenCloseClicked_InvokesOnClose()
    {
        var user = CreateTestUser();
        var onCloseCalled = false;

        var cut = RenderComponent<ViewUserForm>(parameters => parameters
            .Add(p => p.User, user)
            .Add(p => p.OnClose, () => { onCloseCalled = true; }));

        cut.Find("button.btn-secondary").Click();

        onCloseCalled.Should().BeTrue();
    }

    private static UserDto CreateTestUser(bool isActive = true)
    {
        return new UserDto
        {
            Id = 1,
            Forename = "John",
            Surname = "Doe",
            Email = "john@example.com",
            Role = 0,
            IsActive = isActive,
            BirthDate = new DateTime(1990, 1, 1)
        };
    }
}