using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using UserManagement.Services.Domain;
using UserManagement.Services.Validation;

namespace UserManagement.Web.Dtos
{
    /// <summary>
    /// Separate DTO just for patches which has optional qualities and no ID
    /// </summary>
    public class UserPatchDto
    {
        [DefaultValue("John")]
        [MaxLength(50), MinLength(2)]
        public string? Forename { get; set; }

        [DefaultValue("Doe")]
        [MaxLength(50), MinLength(2)]
        public string? Surname { get; set; }

        [DefaultValue("john.doe@example.com")]
        [EmailAddress, MaxLength(100)]
        public string? Email { get; set; }

        [DefaultValue(UserRole.User)]
        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid role specified.")]
        public UserRole? Role { get; set; } = UserRole.User;

        public bool? IsActive { get; set; }

        [DefaultValue("1990-01-01")]
        [BirthDate]
        public DateTime? BirthDate { get; set; }
    }
}
