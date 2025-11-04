using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserManagement.Services.Domain.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    Task<IEnumerable<User>> FilterByActiveAsync(bool isActive);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetByNameAsync(String forename, String surname);
    Task<User?> GetByIdAsync(long id);
    Task<User> AddUserAsync(User user);
    public Task<User> UpdateUserAsync(User user);
    public Task DeleteUserAsync(long id);
    public Task SaveAsync();

}
