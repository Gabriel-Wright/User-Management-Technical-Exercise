using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;

public class UserEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required, MaxLength(50), MinLength(2)]
    public string Forename { get; set; } = default!;
    [Required, MaxLength(50), MinLength(2)]
    public string Surname { get; set; } = default!;
    [Required, MaxLength(100)]
    public string Email { get; set; } = default!;
    public string UserRole { get; set; } = "User";
    public bool IsActive { get; set; }
    public DateTime BirthDate { get; set; }
    // //have this set as a bool
    public bool Deleted { get; set; } = false;
}
