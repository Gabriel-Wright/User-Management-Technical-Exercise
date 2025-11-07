using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserManagement.Services.Domain.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<(IEnumerable<User> Users, int TotalCount)> GetUsersAsync(UserQuery query);
    Task<User?> GetByIdAsync(long id);
    Task<User> AddUserAsync(User user);
    public Task<User> UpdateUserAsync(User user);
    // public Task DeleteUserAsync(long id); //Decided against exposing hard delete user case
    public Task SoftDeleteUserAsync(long id); //Soft delete user preferable for auditing and generally
    public Task SaveAsync();

}
