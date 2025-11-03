using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;

public class Audit
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    // dont think we need navigation property here - what if the entity is deleted
    public long EntityId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public enum AuditAction
    {
        Created,
        Updated,
        Deleted
    }
    [Required]
    public AuditAction Action { get; set; } = default!;
    public string? ChangedBy { get; set; } //Username of admin
    public string? Changes { get; set; }
}