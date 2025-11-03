using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using UserManagement.Services.Domain;

public class UserDtoTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }

    [Fact]
    public void User_ValidModel_ShouldPassValidation()
    {
        var user = new User
        {
            Forename = "John",
            Surname = "Doe",
            Email = "john@example.com",
            Role = UserRole.Admin,
            IsActive = true,
            BirthDate = new DateTime(1990, 5, 1)
        };

        var results = Validate(user);
        results.Should().BeEmpty();
    }

    public void User_WhenFirstNameContainsForeignCharacters_ShouldPassvValidation()
    {
        var user = new User
        {
            Forename = "Jürgen",
            Surname = "Müller",
            Email = "j@gmail.com",
            Role = UserRole.User,
            IsActive = true,
            BirthDate = new DateTime(1985, 7, 15)
        };
        var results = Validate(user);

        results.Should().BeEmpty();
    }

    [Fact]
    public void User_WhenFirstNameMissing_ShouldFailValidation()
    {
        var user = new User
        {
            Forename = "",
            Surname = "Doe",
            Email = "test@example.com",
            BirthDate = DateTime.UtcNow
        };

        var results = Validate(user);

        results.Should().Contain(r =>
            r.MemberNames.Contains(nameof(User.Forename)));
    }

    [Fact]
    public void User_FirstNameTooShort_ShouldFailValidation()
    {
        var user = new User
        {
            Forename = "J",
            Surname = "Doe",
            Email = "test@example.com",
            BirthDate = DateTime.UtcNow
        };

        var results = Validate(user);

        results.Should().Contain(r =>
            r.MemberNames.Contains(nameof(User.Forename)));
    }

    [Fact]
    public void User_FirstNameInvalidCharacters_ShouldFailValidation()
    {
        var user = new User
        {
            Forename = "J0hn!",
            Surname = "Doe",
            Email = "test@example.com",
            BirthDate = DateTime.UtcNow
        };

        var results = Validate(user);

        results.Should().Contain(r =>
            r.MemberNames.Contains(nameof(User.Forename)));
    }

    [Fact]
    public void User_InvalidEmail_ShouldFailValidation()
    {
        var user = new User
        {
            Forename = "John",
            Surname = "Doe",
            Email = "not-an-email",
            BirthDate = DateTime.UtcNow
        };

        var results = Validate(user);

        results.Should().Contain(r =>
            r.MemberNames.Contains(nameof(User.Email)));
    }
}
