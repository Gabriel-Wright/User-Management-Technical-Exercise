using System.ComponentModel.DataAnnotations;

namespace UserManagement.UI.Dtos
{
    public class UserPatchDto
    {
        [Required(ErrorMessage = "First name required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string? Forename { get; set; }
        [Required(ErrorMessage = "Surname required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string? Surname { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email is too long")]
        public string? Email { get; set; }
        public int? Role { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? BirthDate
        {
            get; set;
        }
    }
}