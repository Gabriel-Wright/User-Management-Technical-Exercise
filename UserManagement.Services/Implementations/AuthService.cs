using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Domain.Login;

namespace UserManagement.Services.Domain.Implementations;

public class AuthService : IAuthService
{
    private readonly IDataContext _dataContext;
    private readonly IJwtService _jwtService;
    private readonly string _defaultPassword;


    public AuthService(IDataContext dataContext, IJwtService jwtService, IOptions<UserSettings> userSettings)
    {
        _dataContext = dataContext;
        _jwtService = jwtService;
        _defaultPassword = userSettings.Value.DefaultPassword;
    }

    public async Task<LoginResponse?> AuthenticateAsync(string email, string password)
    {
        Log.Information("Authentication attempt for email: {Email}", email);

        var user = await _dataContext.GetAll<UserEntity>()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
        {
            Log.Warning("Authentication failed: User not found - {Email}", email);
            return null;
        }

        if (!user.IsActive)
        {
            Log.Warning("Authentication failed: User is inactive - {Email}", email);
            return null;
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            Log.Error(
                "Authentication failed: user has no password hash stored - probably user setup wrong. Email={Email}",
                email);
            return null;
        }

        //Check password
        var passwordHasher = new PasswordHasher<UserEntity>();
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            Log.Warning("Authentication failed: Invalid password for {Email}", email);
            return null;
        }

        Log.Information("User {Email} authenticated successfully", email);

        //Generate JWT Token
        var token = _jwtService.GenerateToken(user);

        return new LoginResponse
        {
            Token = token, Email = user.Email, Role = user.UserRole, Expiration = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task<User> SetDefaultUserPasswordAsync(User user)
    {
        return await SetUserPasswordAsync(user, _defaultPassword);
    }

    public async Task<User> SetUserPasswordAsync(User user, string password)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        //Get  corresponding UserEntity from the database
        var userEntity = await _dataContext.GetAll<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (userEntity == null)
            throw new InvalidOperationException($"User with Id {user.Id} not found.");

        //Hash pwd
        var hasher = new PasswordHasher<UserEntity>();
        userEntity.PasswordHash = hasher.HashPassword(userEntity, password);

        _dataContext.UpdateE(userEntity);
        await _dataContext.SaveChangesAsync();

        return user;
    }


}
