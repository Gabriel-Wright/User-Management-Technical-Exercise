using System;
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
        [MaxLength(50), MinLength(2)]
        public string? Forename { get; set; }
        [MaxLength(50), MinLength(2)]
        public string? Surname { get; set; }
        [EmailAddress, MaxLength(100)]
        public string? Email { get; set; }
        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid role specified.")]
        public UserRole? Role { get; set; } = UserRole.User;
        public bool? IsActive { get; set; }
        [BirthDate]
        public DateTime? BirthDate { get; set; }
    }
}
