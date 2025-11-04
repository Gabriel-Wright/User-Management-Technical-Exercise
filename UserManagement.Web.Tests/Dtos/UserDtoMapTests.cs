using System;
using FluentAssertions;
using UserManagement.Services.Domain;
using UserManagement.Web.Dtos;

namespace UserManagement.Tests.Dtos;

public class UserDtoMapTests
{
    [Fact]
    public void ToDto_MapsAllPropertiesCorrectly()
    {
        var user = new User
        {
            Id = 1,
            Forename = "Alice",
            Surname = "In",
            Email = "Wonderland@holeundertree.com",
            Role = UserRole.Admin,
            IsActive = true,
            BirthDate = new DateTime(1990, 1, 1)
        };

        var dto = UserDtoMapper.ToDto(user);

        dto.Should().BeEquivalentTo(user);
    }

    [Fact]
    public void ToUser_MapsAllPropertiesCorrectly()
    {
        var dto = new UserDto
        {
            Id = 2,
            Forename = "Bob",
            Surname = "dylan",
            Email = "bob@1957.com",
            Role = UserRole.User,
            IsActive = false,
            BirthDate = new DateTime(2001, 6, 26)
        };

        var user = UserDtoMapper.ToUser(dto);

        user.Should().BeEquivalentTo(dto);
    }
}
