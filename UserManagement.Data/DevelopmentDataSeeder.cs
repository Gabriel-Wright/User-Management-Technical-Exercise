using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement.Data
{
    /// <summary>
    /// Separate class used to seed data - introduced in order to bring in default password for hashing.
    /// Fine to store DEFAULT_PASSWORD in code here - only development data.
    /// </summary>
    public static class DevelopmentDataSeeder
    {
        private const string DEFAULT_PASSWORD = "!Password1";

        public static void SeedUsers(DataContext context)
        {
            // Only seed if database is empty
            if (context.Users != null && context.Users.Any())
                return;

            var passwordHasher = new PasswordHasher<UserEntity>();

            var users = new[]
            {
                CreateUser(1, "Peter", "Loew", "ploew@example.com", true, -25, "User"),
                CreateUser(2, "Benjamin Franklin", "Gates", "bfgates@example.com", true, -40, "Admin"),
                CreateUser(3, "Castor", "Troy", "ctroy@example.com", false, -35, "User"),
                CreateUser(4, "Memphis", "Raines", "mraines@example.com", true, -50, "User"),
                CreateUser(5, "Stanley", "Goodspeed", "sgodspeed@example.com", true, -29, "User"),
                CreateUser(6, "H.I.", "McDunnough", "himcdunnough@example.com", true, -60, "User"),
                CreateUser(7, "Cameron", "Poe", "cpoe@example.com", false, -33, "User"),
                CreateUser(8, "Edward", "Malus", "emalus@example.com", false, -28, "User"),
                CreateUser(9, "Damon", "Macready", "dmacready@example.com", false, -45, "User"),
                CreateUser(10, "Johnny", "Blaze", "jblaze@example.com", true, -38, "User"),
                CreateUser(11, "Robin", "Feld", "rfeld@example.com", true, -32, "User")
            };

            foreach (var user in users)
            {
                user.PasswordHash = passwordHasher.HashPassword(user, DEFAULT_PASSWORD);
            }

            context.Users!.AddRange(users);
            context.SaveChanges();
        }

        private static UserEntity CreateUser(long id, string forename, string surname, string email,
            bool isActive, int yearsOld, string role)
        {
            return new UserEntity
            {
                Id = id,
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive,
                Deleted = false,
                BirthDate = DateTime.Today.AddYears(yearsOld),
                UserRole = role,
                PasswordHash = default!
            };
        }
    }
}
