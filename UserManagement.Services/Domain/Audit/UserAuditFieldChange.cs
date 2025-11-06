using UserManagement.Services.Domain;

public class UserAuditFieldChange
{
    public UserField FieldName { get; set; } = default!;
    public string? Before { get; set; }                //old value, nullable for create
    public string After { get; set; } = default!;      //new value, required

}
