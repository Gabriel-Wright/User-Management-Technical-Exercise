using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using UserManagement.UI.Dtos;
using UserManagement.UI.Exceptions;
using UserManagement.UI.Services;

public class UserAuditApiServiceTests
{
    private HttpClient CreateHttpClient(Mock<HttpMessageHandler> handlerMock)
    {
        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
    }

    [Fact]
    public async Task GetAuditsByQueryAsync_ReturnsPagedResult_WhenSuccess()
    {
        var pagedResult = new PagedResult<UserAuditDto>
        {
            Items = new List<UserAuditDto>
            {
                new UserAuditDto { Id = 1, Action = "Created", UserEmail = "user1@test.com" },
                new UserAuditDto { Id = 2, Action = "Updated", UserEmail = "user2@test.com" }
            },
            TotalCount = 2
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri!.ToString().Contains("users/audits/query")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(pagedResult)
            });

        var service = new UserAuditApiService(CreateHttpClient(handlerMock));

        var result = await service.GetAuditsByQueryAsync(searchTerm: "test");

        result.Items.Should().HaveCount(2);
        result.Items[0].Action.Should().Be("Created");
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAuditsByQueryAsync_ThrowsUserApiException_OnHttpError()
    {
        //assume we will throw
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Server error")
            });

        var service = new UserAuditApiService(CreateHttpClient(handlerMock));

        //gets handled as an api exception
        await Assert.ThrowsAsync<UserApiException>(() => service.GetAuditsByQueryAsync());
    }

    [Fact]
    public async Task GetAuditsByUserAsync_ThrowsUserApiException_WhenUserIdIsInvalid()
    {
        var service = new UserAuditApiService(new HttpClient());

        await Assert.ThrowsAsync<UserApiException>(() => service.GetAuditsByUserAsync(0));
        await Assert.ThrowsAsync<UserApiException>(() => service.GetAuditsByUserAsync(-5));
    }

    [Fact]
    public async Task GetAuditsByUserAsync_ReturnsPagedResult_WhenSuccess()
    {
        var pagedResult = new PagedResult<UserAuditDto>
        {
            Items = new List<UserAuditDto>
            {
                new UserAuditDto { Id = 10, Action = "Deleted", UserEmail = "deleted@domain.com" }
            },
            TotalCount = 1
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri!.ToString().Contains("users/audits/10")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(pagedResult)
            });

        var service = new UserAuditApiService(CreateHttpClient(handlerMock));

        var result = await service.GetAuditsByUserAsync(10);

        result.Items.Should().ContainSingle();
        result.Items.First().Action.Should().Be("Deleted");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAuditsByUserAsync_ThrowsUserApiException_OnHttpError()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad Request")
            });

        var service = new UserAuditApiService(CreateHttpClient(handlerMock));

        await Assert.ThrowsAsync<UserApiException>(() => service.GetAuditsByUserAsync(10));
    }
}
