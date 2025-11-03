using System;
using UserManagement.Models;
using UserManagement.Services.Domain;

namespace UserManagement.Services.Mappers;

public static class UserMapper
{
    public static User ToDomainUser(UserEntity user)
    {
        return new User
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            Role = Enum.Parse<UserRole>(user.UserRole, ignoreCase: true),
            IsActive = user.IsActive,
            BirthDate = user.BirthDate
        };
    }

    public static UserEntity ToUserEntity(User user)
    {
        return new UserEntity
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email.ToString(),
            UserRole = user.Role.ToString(),
            IsActive = user.IsActive,
            BirthDate = user.BirthDate
        };
    }
}
