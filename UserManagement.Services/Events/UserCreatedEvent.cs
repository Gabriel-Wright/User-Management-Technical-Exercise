using UserManagement.Services.Domain;
using UserManagement.Services.Events;

namespace UserMangement.Services.Events;

public class UserCreatedEvent : IUserDomainEvent
{
    public long UserId { get; set; }
    public required User User { get; set; }
}