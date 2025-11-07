using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;

public class UserAuditEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    public long UserEntityId { get; set; }//Foreign Key Column

    [Required]
    [ForeignKey("UserEntityId")]
    public UserEntity UserEntity { get; set; } = default!;
    [Required]
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public String AuditAction { get; set; } = default!;
    //This won't be a column -> this handled by EF and shows the relationship
    public ICollection<UserAuditChangeEntity> Changes { get; set; } = new List<UserAuditChangeEntity>();

}