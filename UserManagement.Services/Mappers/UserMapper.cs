using System;
using UserManagement.Models;
using UserManagement.Services.Domain;

namespace UserManagement.Services.Mappers;

public static class UserMapper
{
    public static User ToDomainUser(UserEntity userEntity)
    {
        return new User
        {
            Id = userEntity.Id,
            Forename = userEntity.Forename,
            Surname = userEntity.Surname,
            Email = userEntity.Email,
            Role = Enum.Parse<UserRole>(userEntity.UserRole, ignoreCase: true),
            IsActive = userEntity.IsActive,
            BirthDate = userEntity.BirthDate
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
