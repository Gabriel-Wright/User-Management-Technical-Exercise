using System.ComponentModel.DataAnnotations;
using UserManagement.UI.Validation;

namespace UserManagement.UI.Dtos
{
    public class UserCreateDto
    {
        [Required(ErrorMessage = "First name required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string Forename { get; set; } = "John";
        [Required(ErrorMessage = "Surname required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string Surname { get; set; } = "Smith";
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email is too long")]
        public string Email { get; set; } = "example@email.com";
        public int Role { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        [DataType(DataType.Date)]
        [BirthDate(MinAge = 18, MaxAge = 120)]
        public DateTime BirthDate
        {
            get; set;
        } = DateTime.Now;
    }
}