using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using UserManagement.UI.Components;

namespace UserManagement.UI.Tests.Components;

public class UserSearchBarTests : TestContext
{
    [Fact]
    public void UserSearchBar_InitialRender_ShowsSearchInput()
    {
        var cut = RenderComponent<UserSearchBar>();

        cut.Markup.Should().Contain("Search users...");
        //--- , Active, InActive
        cut.FindAll("select option").Should().HaveCount(3);
    }

    [Fact]
    public async Task UserSearchBar_WhenSearchClicked_InvokesOnSearchWithCorrectParameters()
    {
        string? capturedSearchTerm = null;
        bool? capturedIsActive = null;

        var cut = RenderComponent<UserSearchBar>(parameters => parameters
            .Add(p => p.OnSearch, (args) =>
            {
                capturedSearchTerm = args.searchTerm;
                capturedIsActive = args.isActive;
            }));

        cut.Find("#user-search-bar").Change("billy");
        cut.Find("select").Change("true");
        await cut.Find("button.btn-primary").ClickAsync(new());

        capturedSearchTerm.Should().Be("billy");
        capturedIsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UserSearchBar_WhenActiveFilterSetToAll_PassesNullForIsActive()
    {
        bool? capturedIsActive = null;

        var cut = RenderComponent<UserSearchBar>(parameters => parameters
            .Add(p => p.OnSearch, (args) =>
            {
                capturedIsActive = args.isActive;
            }));

        // Act
        cut.Find("select").Change("");
        await cut.Find("button.btn-primary").ClickAsync(new());

        capturedIsActive.Should().BeNull();
    }

    [Fact]
    public async Task UserSearchBar_WhenActiveFilterSetToInactive_PassesFalse()
    {
        bool? capturedIsActive = null;

        var cut = RenderComponent<UserSearchBar>(parameters => parameters
            .Add(p => p.OnSearch, (args) =>
            {
                capturedIsActive = args.isActive;
            }));

        cut.Find("select").Change("false");  // "Inactive" option
        await cut.Find("button.btn-primary").ClickAsync(new());

        capturedIsActive.Should().BeFalse();
    }

    [Fact]
    public void UserSearchBar_WhenSearchTermProvided_DisplaysInInput()
    {
        var cut = RenderComponent<UserSearchBar>(parameters => parameters
            .Add(p => p.SearchTerm, "test search"));

        var input = cut.Find("input[type='text']");
        input.GetAttribute("value").Should().Be("test search");
    }
}