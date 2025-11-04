using System;
using FluentAssertions;
using UserManagement.Services.Domain;
using UserManagement.Web.Dtos;

namespace UserManagement.Tests.Dtos;

public class UserToUserCreateUserMapTests
{
    [Fact]
    public void ToUser_MapsAllFieldsExceptId()
    {
        var createDto = new UserCreateDto
        {
            Forename = "Charlie",
            Surname = "Day",
            Email = "charlie@Williwonka.com",
            Role = UserRole.Admin,
            IsActive = true,
            BirthDate = new DateTime(1995, 3, 15)
        };

        var user = UserToUserCreateDtoMapper.ToUser(createDto);

        user.Should().BeEquivalentTo(createDto, options => options.ExcludingMissingMembers());
        user.Id.Should().Be(0);
    }
}
