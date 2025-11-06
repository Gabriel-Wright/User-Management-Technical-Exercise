using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserManagement.UI.Dtos;
using UserManagement.UI.Pages;

namespace UserManagement.UI.Tests.Pages;

public class UserListTests : TestContext
{
    private readonly Mock<IUserApiService> _mockUserService;

    public UserListTests()
    {
        _mockUserService = new Mock<IUserApiService>();
        Services.AddSingleton(_mockUserService.Object);
    }

    // [Fact]
    // public async Task UserList_OnInitialLoad_ShowsLoadingState()
    // {
    //     var tcs = new TaskCompletionSource<(List<UserDto>, int)>();
    //     _mockUserService
    //         .Setup(s => s.GetUsersByQueryAsync(null, null, 1, 10, "id", false))
    //         .Returns(tcs.Task);

    //     var cut = RenderComponent<UserList>();

    //     cut.WaitForState(() => cut.Markup.Contains("Loading users..."), TimeSpan.FromSeconds(1));

    //     tcs.SetResult((new List<UserDto>(), 0));

    //     await Task.Yield(); //need to run these calls async
    // }

    private static List<UserDto> CreateTestUsers()
    {
        return new List<UserDto>
    {
        new UserDto
        {
            Id = 1,
            Forename = "Bruce",
            Surname = "Wayne",
            Email = "Bruce.Wayne@example.com",
            Role = 0,
            IsActive = true,
            BirthDate = new DateTime(1995, 5, 15)
        },
        new UserDto
        {
            Id = 2,
            Forename = "Clark",
            Surname = "Kent",
            Email = "clark.kent@example.com",
            Role = 1,
            IsActive = true,
            BirthDate = new DateTime(1990, 8, 20)
        },
        new UserDto
        {
            Id = 3,
            Forename = "Paul",
            Surname = "Allen",
            Email = "Paul.Allen@example.com",
            Role = 0,
            IsActive = false,
            BirthDate = new DateTime(1985, 12, 10)
        }
    };
    }
}