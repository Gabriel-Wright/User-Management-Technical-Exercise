using System;
using System.Collections.Generic;

namespace UserManagement.Services.Domain;

public class UserAudit
{
    public long Id { get; set; }
    public long UserId { get; set; }  //Store UserForename, Surname and Email in this audit as well
    //This is necessary as well in case names change
    public string? UserForename { get; set; }
    public string? UserSurname { get; set; }
    public string? UserEmail { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    public AuditAction Action { get; set; } = default!;
    public List<UserAuditChange>? Changes { get; set; } //There doesn't have to be any changes associated 
    //e.g. no change associated for delete.
}