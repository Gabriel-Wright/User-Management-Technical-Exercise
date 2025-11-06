using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;

public class UserAuditChangeEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    public long AuditId { get; set; }//Foreign Key

    [ForeignKey("AuditId")]
    public UserAuditEntity Audit { get; set; } = default!; [Required]
    public string Field { get; set; } = "Created";
    [Required]
    public string? Before { get; set; } //Before is nullable - create will have nothing before
    [Required]
    public string After { get; set; } = ""; //After is not nullable - in case of delete, we won't create AuditChangeEntities
}