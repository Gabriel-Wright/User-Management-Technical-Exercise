namespace UserMangement.Services.Events;

using UserManagement.Services.Events;

public class UserDeletedEvent : IUserDomainEvent
{
    public long UserId { get; set; }
}