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

        public async Task CreateUserUpdatedAuditAsync(long userId, User oldUser, User newUser)
        {
            var audit = new UserAuditEntity
            {
                UserEntityId = userId,
                LoggedAt = DateTime.UtcNow,
                AuditAction = "Updated"
            };
            Log.Information("Creating new Audit for User {id} at {time}. Operation: {action}", audit.UserEntityId, audit.LoggedAt, audit.AuditAction);

            await _dataContext.CreateAsync(audit);
            await SaveAuditChangesAsync();

            var changes = new List<UserAuditChangeEntity>();

            void AddChange(string field, string before, string after)
            {
                if (before != after)
                {
                    changes.Add(new UserAuditChangeEntity
                    {
                        AuditId = audit.Id,
                        Field = field,
                        Before = before,
                        After = after
                    });
                }
            }

            AddChange("Forename", oldUser.Forename, newUser.Forename);
            AddChange("Surname", oldUser.Surname, newUser.Surname);
            AddChange("Email", oldUser.Email, newUser.Email);
            AddChange("Role", oldUser.Role.ToString(), newUser.Role.ToString());
            AddChange("IsActive", oldUser.IsActive.ToString(), newUser.IsActive.ToString());
            AddChange("BirthDate", oldUser.BirthDate.ToString("yyyy-MM-dd"), newUser.BirthDate.ToString("yyyy-MM-dd"));

            foreach (var change in changes)
            {
                Log.Debug("Appending new change for audit {auditId}. Field: {field} - Before: {before} - After: {after}", audit.Id, change.Field, change.Before, change.After);
                await _dataContext.CreateAsync(change);
            }

            await SaveAuditChangesAsync();
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

        public async Task CreateUserDeletedAuditAsync(long userId)
        {
            var audit = new UserAuditEntity
            {
                UserEntityId = userId,
                LoggedAt = DateTime.UtcNow,
                AuditAction = "Deleted"
            };

            Log.Information("Creating new Audit for User {id} at {time}. Operation: {action}", audit.UserEntityId, audit.LoggedAt, audit.AuditAction);
            //Need to save the audit first to get the generated Id
            await _dataContext.CreateAsync(audit);
            await SaveAuditChangesAsync();

            //No need to log specific changes - whole thing was deleted
        }

        public async Task SaveAuditChangesAsync()
        {
            await _dataContext.SaveChangesAsync();
        }

        //Event handlers

        public async Task Handle(UserCreatedEvent evt)
        {
            await CreateUserCreatedAuditAsync(evt.UserId, evt.User);
        }

        public async Task Handle(UserUpdatedEvent evt)
        {
            await CreateUserUpdatedAuditAsync(evt.UserId, evt.OlderUser, evt.NewUser);
        }

        public async Task Handle(UserDeletedEvent evt)
        {
            await CreateUserDeletedAuditAsync(evt.UserId);
        }

    }
}
