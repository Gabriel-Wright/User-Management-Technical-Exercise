using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;

public class AuditEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    // dont think we need navigation property here - what if the entity is deleted
    [Required]
    public long EntityId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    [Required]
    public String AuditAction { get; set; } = default!;
    [MaxLength(100)]
    public string? ChangedBy { get; set; }
    public string? Changes { get; set; }
}