using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtService(IConfiguration configuration)
    {
        //Read from env variable then fall back to JwtSecretKey
        _secretKey = Environment.GetEnvironmentVariable("UMS_JWT_SECRET_KEY")
                     ?? configuration["Ums_Jwt:SecretKey"]
                     ?? throw new InvalidOperationException("JWT key not configured");

        _issuer = configuration["Ums_Jwt:Issuer"] ?? "UserManagementAPI";
        _audience = configuration["Ums_Jwt:Audience"] ?? "UserManagementUI";
        _expirationMinutes = int.Parse(configuration["Ums_Jwt:ExpirationMinutes"] ?? "60");

        Log.Information("JWT Token Service initialized with Issuer: {Issuer}, Audience: {Audience}",
            _issuer, _audience);
    }

    public string GenerateToken(UserEntity user)
    {
        Log.Information("Generating JWT token for user: {Email}", user.Email);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.Forename} {user.Surname}"),
            new Claim(ClaimTypes.Role, user.UserRole),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        Log.Information("JWT token generated successfully for user: {Email}", user.Email);

        return tokenString;
    }
}
