using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;

public class User
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required, MaxLength(50)]
    public string Forename { get; set; } = default!;
    [Required, MaxLength(50)]
    public string Surname { get; set; } = default!;
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = default!;

    public enum Role
    {
        User,
        Admin
    }

    [Required]
    public Role UserRole { get; set; } = Role.User;
    public bool IsActive { get; set; }
    public DateTime BirthDate { get; set; }
}
