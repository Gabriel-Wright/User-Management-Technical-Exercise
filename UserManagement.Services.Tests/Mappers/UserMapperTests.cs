using System;
using UserManagement.Models;
using UserManagement.Services.Domain;
using UserManagement.Services.Mappers;

public class UserMapperTests
{
    [Fact]
    public void MapToUserDomain_ShouldConvertUserEntityToDomainUser()
    {
        var userEntity = new UserEntity
        {
            Id = 1,
            Forename = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            UserRole = "Admin",
            IsActive = true,
            BirthDate = new DateTime(1990, 5, 1)
        };

        var user = UserMapper.ToDomainUser(userEntity);

        // Assert
        user.Id.Should().Be(1);
        user.Forename.Should().Be("John");
        user.Surname.Should().Be("Doe");
        user.Email.Should().Be("john.doe@example.com");
        user.Role.Should().Be(UserRole.Admin);
        user.IsActive.Should().BeTrue();
        user.BirthDate.Should().Be(new DateTime(1990, 5, 1));
    }

    [Fact]
    public void MapToUserEntity_ShouldConvertDomainUserToUserEntity()
    {
        var user = new User
        {
            Id = 1,
            Forename = "Jane",
            Surname = "Doe",
            Email = "jane@example.com",
            Role = UserRole.User,
            IsActive = false,
            BirthDate = new DateTime(1992, 3, 10)
        };

        var userEntity = UserMapper.ToUserEntity(user);

        // Assert
        userEntity.Id.Should().Be(1);
        userEntity.Forename.Should().Be("Jane");
        userEntity.Surname.Should().Be("Doe");
        userEntity.Email.Should().Be("jane@example.com");
        userEntity.UserRole.Should().Be("User");
        userEntity.IsActive.Should().BeFalse();
        userEntity.BirthDate.Should().Be(new DateTime(1992, 3, 10));
    }

}
