using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using UserManagement.Data;
using UserManagement.Models;
using UserMangement.Services.Events;

namespace UserManagement.Services.Domain.Implementations
{
    public class AuditService
    {
        private readonly IDataContext _dataContext;

        public AuditService(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task Handle(UserCreatedEvent evt)
        {
            await CreateUserCreatedAuditAsync(evt.UserId, evt.User);
        }

        public async Task CreateUserCreatedAuditAsync(long userId, User user)
        {
            var audit = new UserAuditEntity
            {
                UserEntityId = userId,
                LoggedAt = DateTime.UtcNow,
                AuditAction = "Created"
            };
            Log.Information("Creating new Audit for User {id} at {time}. Operation: {action}", audit.UserEntityId, audit.LoggedAt, audit.AuditAction);
            //Need to save the audit first to get the generated Id
            await _dataContext.CreateAsync(audit);
            await SaveAuditChangesAsync();

            var changes = new List<UserAuditChangeEntity>
            {
                new UserAuditChangeEntity
                {
                    AuditId = audit.Id,
                    Field = "Forename",
                    Before = string.Empty,
                    After = user.Forename
                },
                new UserAuditChangeEntity
                {
                    AuditId = audit.Id,
                    Field = "Surname",
                    Before = string.Empty,
                    After = user.Surname
                },
                new UserAuditChangeEntity
                {
                    AuditId = audit.Id,
                    Field = "Email",
                    Before = string.Empty,
                    After = user.Email
                },
                new UserAuditChangeEntity
                {
                    AuditId = audit.Id,
                    Field = "Role",
                    Before = string.Empty,
                    After = user.Role.ToString()
                },
                new UserAuditChangeEntity
                {
                    AuditId = audit.Id,
                    Field = "IsActive",
                    Before = string.Empty,
                    After = user.IsActive.ToString()
                },
                new UserAuditChangeEntity
                {
                    AuditId = audit.Id,
                    Field = "BirthDate",
                    Before = string.Empty,
                    After = user.BirthDate.ToString("yyyy-MM-dd")
                }
            };

            foreach (var change in changes)
            {
                Log.Debug("Appending new change for audit {auditId}. Field: {field} - Before: {before} - After: {after}", audit.Id, change.Field, change.Before, change.After);
                await _dataContext.CreateAsync(change);
            }

            await SaveAuditChangesAsync();
        }

        public async Task SaveAuditChangesAsync()
        {
            await _dataContext.SaveChangesAsync();
        }
    }
}
