using System.Threading.Tasks;
using UserManagement.Services.Domain.Login;

namespace UserManagement.Services.Domain.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> AuthenticateAsync(string email, string password); //login
    Task<bool> RegisterAsync(RegisterUserRequest request); //register / create user
}
