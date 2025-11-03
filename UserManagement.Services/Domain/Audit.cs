using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Services.Domain;

public class Audit
{
    public long Id { get; set; }
    public long EntityId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    [Required]
    public AuditAction Action { get; set; } = AuditAction.Viewed!;

    [MaxLength(100)]
    public string? ChangedBy { get; set; }
    public string? Changes { get; set; }

}