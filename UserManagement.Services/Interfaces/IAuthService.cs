using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.Data;
using UserManagement.Services.Domain.Login;

namespace UserManagement.Services.Domain.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> AuthenticateAsync(string email, string password); //login
    Task<User> SetDefaultUserPasswordAsync(User user);
    Task<User> SetUserPasswordAsync(User user, string password);
}
