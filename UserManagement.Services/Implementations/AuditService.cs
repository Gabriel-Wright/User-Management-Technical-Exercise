using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Mappers;
using UserMangement.Services.Events;

namespace UserManagement.Services.Domain.Implementations
{
    public class AuditService : IAuditService
    {
        private readonly IDataContext _dataContext;

        public AuditService(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<(IEnumerable<UserAudit> userAudits, int totalCount)> GetAuditsByQueryAsync(UserAuditQuery passedQuery)
        {
            //Gonna include User Entity and changes here, so we are searching by Joined data.
            var auditsQuery = _dataContext.GetAll<UserAuditEntity>()
                .AsTracking()
                .Include(a => a.UserEntity)
                .Include(a => a.Changes)
                .IgnoreQueryFilters()
                .AsQueryable();

            if (passedQuery.Action.HasValue)
            {
                var actionStr = passedQuery.Action.Value.ToString();
                auditsQuery = auditsQuery.Where(a => a.AuditAction == actionStr);
            }

            //simple property search on user's forename, surname, email individually
            //if expanded would combine into single search term
            if (!string.IsNullOrWhiteSpace(passedQuery.SearchTerm))
            {
                var term = passedQuery.SearchTerm.Trim().ToLower();
                auditsQuery = auditsQuery.Where(a => a.UserEntity != null &&
                    (a.UserEntity.Forename.ToLower().Contains(term) ||
                     a.UserEntity.Surname.ToLower().Contains(term) ||
                     a.UserEntity.Email.ToLower().Contains(term)));
            }

            //For now order by date as default
            auditsQuery = auditsQuery.OrderByDescending(a => a.LoggedAt);

            var totalCount = await auditsQuery.CountAsync();

            var pagedAudits = await auditsQuery
                .Skip((passedQuery.Page - 1) * passedQuery.PageSize)
                .Take(passedQuery.PageSize)
                .ToListAsync();

            return (pagedAudits.Select(UserAuditMapper.ToDomainAudit), totalCount);
        }

        public async Task<(IEnumerable<UserAudit>, int totalCount)> GetAllUserAudits(int page, int pageSize)
        {
            //Should move this into a separate func
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 20) pageSize = 20;

            Log.Debug("Fetching paged audits. Page: {page}, num per page: {size}", page, pageSize);

            var query = _dataContext.GetAll<UserAuditEntity>()
            .AsTracking()
                .Include(a => a.Changes)
                .Include(a => a.UserEntity)
                .IgnoreQueryFilters()
                .OrderByDescending(a => a.LoggedAt);

            var totalCount = await query.CountAsync();

            var pagedAudits = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var audits = pagedAudits.Select(UserAuditMapper.ToDomainAudit);

            return (audits, totalCount);
        }

        public async Task<(IEnumerable<UserAudit>, int totalCount)> GetAllUserAuditsById(long id, int page, int pageSize)
        {
            //Should move this into a separate func
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 20) pageSize = 20;

            Log.Debug("Fetching paged audits. Page: {page}, num per page: {size}", page, pageSize);

            var query = _dataContext.GetAll<UserAuditEntity>()
            .Where(a => a.UserEntityId == id)
            .Include(a => a.UserEntity)
            .Include(a => a.Changes)
            .IgnoreQueryFilters()
                .OrderByDescending(a => a.LoggedAt);

            var totalCount = await query.CountAsync();

            var pagedAudits = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var audits = pagedAudits.Select(UserAuditMapper.ToDomainAudit);

            return (audits, totalCount);
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


            AddChange(changes, audit.Id, "Forename", oldUser.Forename, newUser.Forename);
            AddChange(changes, audit.Id, "Surname", oldUser.Surname, newUser.Surname);
            AddChange(changes, audit.Id, "Email", oldUser.Email, newUser.Email);
            AddChange(changes, audit.Id, "Role", oldUser.Role.ToString(), newUser.Role.ToString());
            AddChange(changes, audit.Id, "IsActive", oldUser.IsActive.ToString(), newUser.IsActive.ToString());
            AddChange(changes, audit.Id, "BirthDate", oldUser.BirthDate.ToString("yyyy-MM-dd"), newUser.BirthDate.ToString("yyyy-MM-dd"));

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

            var changes = new List<UserAuditChangeEntity>();
            AddChange(changes, audit.Id, "Forename", string.Empty, user.Forename);
            AddChange(changes, audit.Id, "Surname", string.Empty, user.Surname);
            AddChange(changes, audit.Id, "Email", string.Empty, user.Email);
            AddChange(changes, audit.Id, "Role", string.Empty, user.Role.ToString());
            AddChange(changes, audit.Id, "IsActive", string.Empty, user.IsActive.ToString());
            AddChange(changes, audit.Id, "BirthDate", string.Empty, user.BirthDate.ToString("yyyy-MM-dd"));

            foreach (var change in changes)
            {
                Log.Debug("Appending new change for audit {auditId}. Field: {field} - Before: {before} - After: {after}", audit.Id, change.Field, change.Before, change.After);
                await _dataContext.CreateAsync(change);
            }

            await SaveAuditChangesAsync();
        }

        private void AddChange(List<UserAuditChangeEntity> changes, long auditId, string field, string before, string after)
        {
            if (before != after)
            {
                changes.Add(new UserAuditChangeEntity
                {
                    AuditId = auditId,
                    Field = field,
                    Before = before,
                    After = after
                });
            }
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



        private void ValidateQuery(UserAuditQuery query)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.PageSize <= 0) query.PageSize = 10;
            if (query.PageSize > 20) query.PageSize = 20; //enforcing a maximum here
            if (string.IsNullOrWhiteSpace(query.SortBy)) query.SortBy = "Id";
        }

    }
}
