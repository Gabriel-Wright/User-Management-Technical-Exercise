using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Mappers;

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
    private readonly IDataContext _dataAccess;
    public UserService(IDataContext dataAccess) => _dataAccess = dataAccess;

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        Log.Debug("Fetching all Users from DB.");
        var userEntities = await _dataAccess.GetAll<UserEntity>().ToListAsync();
        return userEntities.Select(UserMapper.ToDomainUser);
    }

    //Getting all users but passing a query down
    public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersAsync(UserQuery query)
    {
        ValidateQuery(query);
        var usersQuery = _dataAccess.GetAll<UserEntity>().AsQueryable();

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
        usersQuery = query.SortBy.ToLower() switch
        {
            "forename" => query.SortDescending ? usersQuery.OrderByDescending(u => u.Forename) : usersQuery.OrderBy(u => u.Forename),
            "surname" => query.SortDescending ? usersQuery.OrderByDescending(u => u.Surname) : usersQuery.OrderBy(u => u.Surname),
            "email" => query.SortDescending ? usersQuery.OrderByDescending(u => u.Email) : usersQuery.OrderBy(u => u.Email),
            _ => query.SortDescending ? usersQuery.OrderByDescending(u => u.Id) : usersQuery.OrderBy(u => u.Id)
        };

        //Actually returning the list of userEntities
        var usersPage = await usersQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        //Returning mapped over userentities -> DomainUser
        return (usersPage.Select(UserMapper.ToDomainUser), totalCount);
    }

    public async Task<IEnumerable<User>> FilterByActiveAsync(bool isActive)
    {
        Log.Debug("Fetching all Users from DB that are {Active}.", isActive);
        var userEntities = await _dataAccess.GetAll<UserEntity>()
            .Where(u => u.IsActive == isActive)
            .ToListAsync();

        return userEntities.Select(UserMapper.ToDomainUser);
    }

    public async Task<IEnumerable<User>> GetByNameAsync(string forename, string surname)
    {
        Log.Debug("Attempting to get all users of Name {forename}, {surname}", forename, surname);
        if (string.IsNullOrWhiteSpace(forename))
        {
            Log.Error("GetByNameAsync called with empty forename: {forename}", forename);
            throw new ArgumentException("Forename cannot be empty.", nameof(forename));
        }
        if (string.IsNullOrWhiteSpace(surname))
        {
            Log.Error("GetByNameAsync called with empty surname: {surname}", surname);
            throw new ArgumentException("Surname cannot be empty.", nameof(surname));
        }
        forename = forename.Trim();
        surname = surname.Trim();

        Log.Information("Fetching user of name: {forename} {surname}", forename, surname);
        var userEntities = await _dataAccess.GetAll<UserEntity>()
            .Where(u => u.Forename.ToLower() == forename.ToLower() &&
                        u.Surname.ToLower() == surname.ToLower())
            .ToListAsync();
        return userEntities.Select(UserMapper.ToDomainUser);
    }
    public async Task<User?> GetByIdAsync(long id)
    {
        Log.Debug("Fetching user with ID: {id}", id);
        var userEntity = await _dataAccess.GetAll<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == id);

        return userEntity == null ? null : UserMapper.ToDomainUser(userEntity);
    }

    public async Task<User> AddUserAsync(User user)
    {
        Log.Debug("Attempting to add user: {@user}", user);
        ValidateUser(user);

        var existing = await _dataAccess.GetAll<UserEntity>()
        .FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower());

        if (existing != null)
        {
            Log.Error("When attempting to create new user - found that user with email {email} already exists.", user.Email);
            throw new InvalidOperationException($"User with email '{user.Email}' already exists.");
        }


        Log.Information("Adding user {Id}, {Forename} {Surname} to DB", user.Id, user.Forename, user.Surname);
        var userEntity = UserMapper.ToUserEntity(user);
        await _dataAccess.CreateAsync(userEntity);

        return UserMapper.ToDomainUser(userEntity);
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        Log.Debug("Attempting to update user: {@user}", user);
        ValidateUser(user);

        Log.Information("Updating user {Id}, {Forename} {Surname} to DB", user.Id, user.Forename, user.Surname);
        var existing = await _dataAccess.GetAll<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        //Check email uniqueness - would be nice if could reuse a funct here to check this.
        //But also need to check whether Id is the same - unlike in Add.
        var duplicateEmailUser = await _dataAccess.GetAll<UserEntity>()
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

        //Is manually setting entity values here bad?
        existing.Forename = user.Forename;
        existing.Surname = user.Surname;
        existing.Email = user.Email;
        existing.IsActive = user.IsActive;
        existing.UserRole = user.Role.ToString();
        existing.BirthDate = user.BirthDate;

        _dataAccess.UpdateE(existing);
        return UserMapper.ToDomainUser(existing);
    }

    public async Task DeleteUserAsync(long id)
    {
        Log.Debug("Deleting user with Id {id}.", id);
        var existing = await _dataAccess.GetAll<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (existing == null)
        {
            Log.Error("User with Id {id} not found.", id);
            throw new KeyNotFoundException($"User with ID {id} not found.");
        }

        _dataAccess.Delete(existing);
    }

    public async Task SoftDeleteUserAsync(long id)
    {
        var existing = await _dataAccess.GetAll<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (existing == null)
            throw new KeyNotFoundException($"User with ID {id} not found.");

        existing.Deleted = true;
        _dataAccess.UpdateE(existing);
        await _dataAccess.SaveChangesAsync();
    }

    public async Task SaveAsync()
    {
        Log.Information("Saving changes to DB.");
        await _dataAccess.SaveChangesAsync();
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
}

