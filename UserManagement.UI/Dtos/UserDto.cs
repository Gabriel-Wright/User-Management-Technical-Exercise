namespace UserManagement.UI.Dtos;

public class UserDto
{
    public long Id { get; set; }
    public string Forename { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Role { get; set; } = 0;
    public DateTime BirthDate { get; set; }
}