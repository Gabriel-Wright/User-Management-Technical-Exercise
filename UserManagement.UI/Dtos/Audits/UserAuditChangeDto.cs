namespace UserManagement.UI.Dtos;

public class UserAuditChangeDto
{
    public string FieldName { get; set; } = default!;
    public string? Before { get; set; }
    public string? After { get; set; }
}