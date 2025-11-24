using System;
using System.ComponentModel.DataAnnotations;
using UserManagement.Services.Validation;

namespace UserManagement.Services.Domain.Login;

//Default created users to inactive, User Role
public class RegisterUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]

    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
    public string Forename { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    [BirthDate(MinAge = 18, MaxAge = 120)]
    public DateTime? BirthDate { get; set; }
}
