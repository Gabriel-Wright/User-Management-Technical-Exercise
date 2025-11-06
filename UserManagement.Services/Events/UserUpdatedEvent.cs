using UserManagement.Services.Domain;
using UserManagement.Services.Events;

namespace UserMangement.Services.Events;

public class UserUpdatedEvent : IUserDomainEvent
{
    public long UserId { get; set; }
    public required User OlderUser { get; set; }
    public required User NewUser { get; set; }
}

