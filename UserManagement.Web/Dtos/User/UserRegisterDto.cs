using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using UserManagement.Services.Domain;
using UserManagement.Services.Validation;

namespace UserManagement.Web.Dtos
{
    public class UserRegisterDto : UserCreateDto
    {
        [Required]
        [NotNull]
        public required string Password { get; set; }
    }
}
