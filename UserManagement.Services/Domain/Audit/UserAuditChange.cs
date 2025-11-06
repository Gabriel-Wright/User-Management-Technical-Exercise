namespace UserManagement.Services.Domain;

public class UserAuditChange
{
    public long Id { get; set; }
    public long AuditId { get; set; }
    public UserAuditFieldChange Change { get; set; } = default!;

}