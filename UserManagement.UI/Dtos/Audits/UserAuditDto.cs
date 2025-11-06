namespace UserManagement.UI.Dtos;

public class UserAuditDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public DateTime LoggedAt { get; set; }
    public string Action { get; set; } = default!;
    public List<UserAuditChangeDto> Changes { get; set; } = new();
}