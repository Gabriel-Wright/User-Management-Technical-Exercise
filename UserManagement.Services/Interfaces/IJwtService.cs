using UserManagement.Models;

namespace UserManagement.Services.Domain.Interfaces;

public interface IJwtService
{
    string GenerateToken(UserEntity user);
}
