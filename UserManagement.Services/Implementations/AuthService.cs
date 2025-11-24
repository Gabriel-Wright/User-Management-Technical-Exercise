using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    public AuthService(IDataContext dataContext, IJwtService jwtService)
    {
        _dataContext = dataContext;
        _jwtService = jwtService;
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
                Token = token,
                Email = user.Email,
                Role = user.UserRole,
                Expiration = DateTime.UtcNow.AddMinutes(60)
            };
        }

        public async Task<bool> RegisterAsync(RegisterUserRequest request)
        {
            Log.Information("Registration attempt for email: {Email}", request.Email);

            //Check if user exists
            var existingUser = await _dataContext.GetAll<UserEntity>()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (existingUser != null)
            {
                Log.Warning("Registration failed: Email already exists - {Email}", request.Email);
                return false;
            }

            var passwordHasher = new PasswordHasher<UserEntity>();
            var user = new UserEntity
            {
                Email = request.Email,
                Forename = request.Forename,
                Surname = request.Surname,
                PasswordHash = passwordHasher.HashPassword(null!, request.Password),
                UserRole = "User", //Always set role as User for self-registration
                IsActive = true, //Always have InActive set to true
                BirthDate = request.BirthDate ?? DateTime.UtcNow.AddYears(-18),
                Deleted = false
            };

            await _dataContext.CreateAsync(user);
            await _dataContext.SaveChangesAsync();

            Log.Information("User registered successfully: {Email}", request.Email);
            return true;
        }}
