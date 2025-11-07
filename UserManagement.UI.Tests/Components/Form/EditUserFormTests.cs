using System;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserManagement.UI.Components;
using UserManagement.UI.Dtos;
using UserManagement.UI.Exceptions;

namespace UserManagement.UI.Tests.Components;

public class EditUserFormTests : TestContext
{
    private readonly Mock<IUserApiService> _mockUserService;

    public EditUserFormTests()
    {
        _mockUserService = new Mock<IUserApiService>();
        Services.AddSingleton(_mockUserService.Object);
    }


    [Fact]
    public void EditUserForm_PopulatesFormWithUserData()
    {
        var user = CreateTestUser();

        var cut = RenderComponent<EditUserForm>(parameters => parameters
            .Add(p => p.User, user));

        //User gets loaded in properly
        cut.Markup.Should().Contain("Edit User");
        cut.Markup.Should().Contain("Thriller@Man.com");
        cut.Markup.Should().Contain("Michael");
    }

    [Fact]
    public async Task EditUserForm_WhenUpdateFails409_ShowsConflictError()
    {
        var user = CreateTestUser();

        _mockUserService
            .Setup(s => s.PatchUserAsync(user.Id, It.IsAny<UserPatchDto>()))
            .ThrowsAsync(new UserApiException("Conflict", 409));

        var cut = RenderComponent<EditUserForm>(parameters => parameters
            .Add(p => p.User, user));

        await cut.Find("form").SubmitAsync();

        cut.Markup.Should().Contain("A user with this email already exists");
    }

    [Fact]
    public async Task EditUserForm_WhenUpdateFails404_ShowsNotFoundError()
    {
        var user = CreateTestUser();

        _mockUserService
            .Setup(s => s.PatchUserAsync(user.Id, It.IsAny<UserPatchDto>()))
            .ThrowsAsync(new UserApiException("Not found", 404));

        var cut = RenderComponent<EditUserForm>(parameters => parameters
            .Add(p => p.User, user));

        await cut.Find("form").SubmitAsync();

        cut.Markup.Should().Contain("User no longer exists");
    }

    [Fact]
    public void EditUserForm_WhenCloseClicked_InvokesOnClose()
    {
        var user = CreateTestUser();
        var onCloseCalled = false;

        var cut = RenderComponent<EditUserForm>(parameters => parameters
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
            Forename = "Michael",
            Surname = "Jackson",
            Email = "Thriller@Man.com",
            Role = 0,
            IsActive = true,
            BirthDate = new DateTime(1990, 1, 1)
        };
    }
}