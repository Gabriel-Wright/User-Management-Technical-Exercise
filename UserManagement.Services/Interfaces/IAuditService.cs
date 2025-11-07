using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserMangement.Services.Events;

namespace UserManagement.Services.Domain.Interfaces;

public interface IAuditService
{
    Task<(IEnumerable<UserAudit> userAudits, int totalCount)> GetAuditsByQueryAsync(UserAuditQuery passedQuery);
    Task<(IEnumerable<UserAudit>, int totalCount)> GetAllUserAuditsById(long id, int page, int pageSize);
    Task CreateUserUpdatedAuditAsync(long userId, User oldUser, User newUser);
    Task CreateUserCreatedAuditAsync(long userId, User user);
    Task CreateUserDeletedAuditAsync(long userId);
    Task SaveAuditChangesAsync();


    Task Handle(UserCreatedEvent evt);
    Task Handle(UserDeletedEvent evt);
    Task Handle(UserUpdatedEvent evt);


}
