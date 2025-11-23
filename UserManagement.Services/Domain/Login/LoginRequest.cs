using System.ComponentModel.DataAnnotations;

namespace UserManagement.Services.Domain.Login;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}
