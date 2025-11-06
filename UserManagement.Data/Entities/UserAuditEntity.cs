using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;

public class UserAuditEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    public long UserEntityId { get; set; }//Foreign Key Column

    [ForeignKey("UserEntityId")]
    public UserEntity UserEntity { get; set; } = default!;
    [Required]
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public String AuditAction { get; set; } = default!;
}