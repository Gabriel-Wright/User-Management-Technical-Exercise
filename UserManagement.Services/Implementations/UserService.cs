using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Events;
using UserManagement.Services.Mappers;
using UserMangement.Services.Events;

namespace UserManagement.Services.Domain.Implementations;
/// <summary>
/// Service layer to go from Business logic to Data Layer -> including creation, update, retrieval, filtering, and deletion.
///
/// Validation is performed here in service methods by a dedicated private method ValidateUser,.
/// ValidationException is thrown if domain object is invalid. Exceptions then bubble up
/// to the controller calling the Service Layer.
/// Idea here is to keeps the service focused on business logic and have exception handling and additional confirmation / failure
/// logging above.
/// CRUD operations here don't not automatically call SaveChangesAsync. This is for greater control, less coupled behaviour.
/// </summary>
public class UserService : IUserService
{
    private readonly IDataContext _dataContext;
    private readonly IEventBus _eventBus;
    private readonly string _defaultPassword;
    public UserService(IDataContext dataContext, IEventBus eventBus, IOptions<UserSettings> userSettings)
    {
        _dataContext = dataContext;
        _eventBus = eventBus;
        _defaultPassword = userSettings.Value.DefaultPassword;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        Log.Information("Fetching all Users.");
        var userEntities = await _dataContext.GetAll<UserEntity>().ToListAsync();
        return userEntities.Select(UserMapper.ToDomainUser);
    }

    //Getting all users but passing a query down
    public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersAsync(UserQuery query)
    {
        ValidateQuery(query);
        var usersQuery = _dataContext.GetAll<UserEntity>().AsQueryable();

        //Filter by IsActive
        if (query.IsActive.HasValue)
            usersQuery = usersQuery.Where(u => u.IsActive == query.IsActive.Value);

        //Search across forename surname email
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            usersQuery = usersQuery.Where(u =>
                u.Forename.ToLower().Contains(term) ||
                u.Surname.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term));
        }

        //Total count before paging - needed for paging
        var totalCount = await usersQuery.CountAsync();

        //Sorting
        usersQuery = ApplySorting(usersQuery, query.SortBy, query.SortDescending);

        //Actually returning the list of userEntities
        var usersPage = await usersQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        //Returning mapped over userEntities -> DomainUser
        return (usersPage.Select(UserMapper.ToDomainUser), totalCount);
    }

    //This is needed since searching by Id can sometimes be necessary for other service calls
    // e.g. updating
    public async Task<User?> GetByIdAsync(long id)
    {
        Log.Information("Fetching user with ID: {id}", id);
        var userEntity = await _dataContext.GetAll<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == id);

        return userEntity == null ? null : UserMapper.ToDomainUser(userEntity);
    }
    public async Task<User> AddUserAsync(User user)
    {
        Log.Information("Attempting to add user: {@user}", user);
        ValidateUser(user);

        var existing = await _dataContext.GetAll<UserEntity>()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower());

        if (existing != null)
        {
            Log.Error("When attempting to create new user - found that user with email {email} already exists.", user.Email);
            throw new InvalidOperationException($"User with email '{user.Email}' already exists.");
        }

        Log.Information("Adding user {Forename} {Surname} to DB", user.Forename, user.Surname);

        var userEntity = UserMapper.ToUserEntity(user);

        EnsurePasswordHash(userEntity);

        await _dataContext.CreateAsync(userEntity);
        await SaveAsync();

        user.Id = userEntity.Id;
        await _eventBus.PublishAsync(new UserCreatedEvent
        {
            UserId = userEntity.Id,
            User = user
        });

        return user;
    }

    //Ensures user has a valid password hash -> if not use default password. Which is expected rn
    private void EnsurePasswordHash(UserEntity userEntity)
    {
        if (string.IsNullOrEmpty(userEntity.PasswordHash))
        {
            var passwordHasher = new PasswordHasher<UserEntity>();
            userEntity.PasswordHash = passwordHasher.HashPassword(userEntity, _defaultPassword);

            Log.Information("Default password set for user {Email}.", userEntity.Email);
        }
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        ValidateUser(user);

        Log.Information("Updating user {Id}, {Forename} {Surname} to DB", user.Id, user.Forename, user.Surname);
        var existing = await _dataContext.GetAll<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        var duplicateEmailUser = await _dataContext.GetAll<UserEntity>()
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == user.Email.ToLower() &&
                u.Id != user.Id);

        if (duplicateEmailUser != null)
        {
            Log.Error("Email '{email}' is already in use by another user (Id {otherId}).",
                user.Email, duplicateEmailUser.Id);
            throw new InvalidOperationException($"User with email '{user.Email}' already exists.");
        }

        if (existing == null)
        {
            Log.Error("User with Id {id} not found.", user.Id);
            throw new KeyNotFoundException($"User with ID {user.Id} not found.");
        }

        var oldUser = UserMapper.ToDomainUser(existing);

        existing.Forename = user.Forename;
        existing.Surname = user.Surname;
        existing.Email = user.Email;
        existing.IsActive = user.IsActive;
        existing.UserRole = user.Role.ToString();
        existing.BirthDate = user.BirthDate;

        _dataContext.UpdateE(existing);
        await SaveAsync();

        user.Id = existing.Id;
        await _eventBus.PublishAsync(new UserUpdatedEvent
        {
            UserId = user.Id,
            OlderUser = oldUser,
            NewUser = user
        });

        return UserMapper.ToDomainUser(existing);
    }

    public async Task SoftDeleteUserAsync(long id)
    {
        Log.Information("Soft deleting user with ID: {id}", id);
        var existing = await _dataContext.GetAll<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (existing == null)
            throw new KeyNotFoundException($"User with ID {id} not found.");

        existing.Deleted = true;
        _dataContext.UpdateE(existing);
        await _dataContext.SaveChangesAsync();

        await _eventBus.PublishAsync(new UserDeletedEvent
        {
            UserId = id
        });
    }

    public async Task SaveAsync()
    {
        Log.Information("Saving changes to DB.");
        await _dataContext.SaveChangesAsync();
    }

    //Needed to check EF validation
    private void ValidateUser(User user)
    {
        var context = new ValidationContext(user);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(user, context, results, true))
        {
            Log.Error("Validation of user: {Id}, {forename}, {surname} failed", user.Id, user.Forename, user.Surname);
            var errors = string.Join("; ", results.Select(r => r.ErrorMessage));
            throw new ValidationException(errors);
        }
    }

    private void ValidateQuery(UserQuery query)
    {
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize <= 0) query.PageSize = 10;
        if (query.PageSize > 20) query.PageSize = 20; //enforcing a maximum here
        if (string.IsNullOrWhiteSpace(query.SortBy)) query.SortBy = "Id";
    }

    private IQueryable<UserEntity> ApplySorting(IQueryable<UserEntity> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "forename" => descending ? query.OrderByDescending(u => u.Forename) : query.OrderBy(u => u.Forename),
            "surname" => descending ? query.OrderByDescending(u => u.Surname) : query.OrderBy(u => u.Surname),
            "email" => descending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            _ => descending ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id)
        };
    }

}

