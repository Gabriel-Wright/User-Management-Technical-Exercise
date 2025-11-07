using System;
using System.ComponentModel.DataAnnotations;
using UserManagement.Services.Validation;

namespace UserManagement.Services.Domain;

//<summary>
// Added Domain Objects for each entity to try separate data layer from business logic
//</summary>
public class User
{
    public long Id { get; set; }
    [Required(ErrorMessage = "First name required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
    //Extra Regular Expression for special characters is for names like O'Connor or Anne-Marie
    public string Forename { get; set; } = string.Empty;

    [Required(ErrorMessage = "Surname required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
    public string Surname { get; set; } = default!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100)]
    public string Email { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    [BirthDate(MinAge = 18, MaxAge = 120)]
    public DateTime BirthDate { get; set; }


}