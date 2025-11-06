using System;
using System.Collections.Generic;

namespace UserManagement.Services.Domain;

public class UserAudit
{
    public long Id { get; set; }
    public long UserId { get; set; }  //Domain User's ID
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    public AuditAction Action { get; set; } = default!;
    public List<UserAuditChange>? Changes { get; set; } //There doesn't have to be any changes associated 
    //e.g. no change associated for delete.
}