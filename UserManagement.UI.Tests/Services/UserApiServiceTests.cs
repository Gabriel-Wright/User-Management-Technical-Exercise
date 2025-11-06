using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using UserManagement.UI.Dtos;
using UserManagement.UI.Exceptions;
using UserManagement.UI.Services;

public class UserApiServiceTests
{
    private HttpClient CreateHttpClient(Mock<HttpMessageHandler> handlerMock)
    {
        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        return client;
    }

    [Fact]
    public async Task GetUsersByQueryAsync_ReturnsUsers_WhenSuccess()
    {
        var pagedResult = new PagedResult<UserDto>
        {
            Items = new List<UserDto>
            {
                new UserDto { Id = 1, Forename = "Bruce", Surname = "Wayne" },
                new UserDto { Id = 2, Forename = "Clark", Surname = "Kent" }
            },
            TotalCount = 2
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(pagedResult)
            });

        var service = new UserApiService(CreateHttpClient(handlerMock));

        var (users, totalCount) = await service.GetUsersByQueryAsync();

        users.Should().HaveCount(2);
        users[0].Forename.Should().Be("Bruce");
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetUsersByQueryAsync_ThrowsUserApiException_OnHttpError()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Server error")
            });

        var service = new UserApiService(CreateHttpClient(handlerMock));

        await Assert.ThrowsAsync<UserApiException>(() => service.GetUsersByQueryAsync());
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsCreatedUser_WhenSuccess()
    {
        var userToCreate = new UserCreateDto { Forename = "Bruce", Surname = "Wayne", Email = "bruce@example.com" };
        var createdUser = new UserDto { Id = 1, Forename = "Bruce", Surname = "Wayne" };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post && r.RequestUri!.PathAndQuery == "/users"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = JsonContent.Create(createdUser)
            });

        var service = new UserApiService(CreateHttpClient(handlerMock));

        var result = await service.CreateUserAsync(userToCreate);

        result.Id.Should().Be(1);
        result.Forename.Should().Be("Bruce");
    }
}
