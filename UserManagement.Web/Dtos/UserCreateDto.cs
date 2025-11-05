using System;
using System.ComponentModel.DataAnnotations;
using UserManagement.Services.Domain;
using UserManagement.Services.Validation;

namespace UserManagement.Web.Dtos
{
    public class UserCreateDto
    {
        [Required, MaxLength(50), MinLength(2)]
        public string Forename { get; set; } = string.Empty;

        [Required, MaxLength(50), MinLength(2)]
        public string Surname { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid role specified.")]
        public UserRole Role { get; set; } = UserRole.User;
        public bool IsActive { get; set; }
        [BirthDate]
        public DateTime BirthDate { get; set; }
    }
}
