using UserManagement.Services.Domain;

namespace UserManagement.Web.Dtos
{
    public static class UserDtoMapper
    {
        public static UserDto ToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Forename = user.Forename,
                Surname = user.Surname,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                BirthDate = user.BirthDate
            };
        }

        public static User ToUser(UserDto userDto)
        {
            return new User
            {
                Id = userDto.Id,
                Forename = userDto.Forename,
                Surname = userDto.Surname,
                Email = userDto.Email,
                Role = userDto.Role,
                IsActive = userDto.IsActive,
                BirthDate = userDto.BirthDate
            };
        }
    }
}
