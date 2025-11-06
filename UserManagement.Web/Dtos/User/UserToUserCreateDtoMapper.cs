using UserManagement.Services.Domain;

namespace UserManagement.Web.Dtos;

public class UserToUserCreateDtoMapper
{
    public static User ToUser(UserCreateDto dto)
    {
        return new User
        {
            //Id is NOT set here with create -- EF will generate it
            Forename = dto.Forename,
            Surname = dto.Surname,
            Email = dto.Email,
            Role = dto.Role,
            IsActive = dto.IsActive,
            BirthDate = dto.BirthDate
        };
    }
}