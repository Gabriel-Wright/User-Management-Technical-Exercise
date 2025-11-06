using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using UserManagement.Services.Domain;
using UserManagement.Services.Validation;

namespace UserManagement.Web.Dtos
{
    public class UserCreateDto
    {
        [DefaultValue("John")]
        [Required, MaxLength(50), MinLength(2)]
        public string Forename { get; set; } = string.Empty;

        [DefaultValue("Doe")]
        [Required, MaxLength(50), MinLength(2)]
        public string Surname { get; set; } = string.Empty;

        [DefaultValue("john.doe@example.com")]
        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        [DefaultValue(UserRole.User)]

        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid role specified.")]
        public UserRole Role { get; set; } = UserRole.User;

        public bool IsActive { get; set; }

        [DefaultValue("1990-01-01")]
        [BirthDate]
        public DateTime BirthDate { get; set; }
    }
}
