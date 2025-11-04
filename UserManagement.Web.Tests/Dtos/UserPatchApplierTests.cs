using System;
using UserManagement.Services.Domain;
using UserManagement.Web.Dtos;

namespace UserManagement.Tests.Dtos;

public class UserPatchApplierTests
{
    [Fact]
    public void ApplyPatch_UpdatesOnlyProvidedFields()
    {
        var existingUser = new User
        {
            Id = 1,
            Forename = "Old",
            Surname = "Name",
            Email = "old@example.com",
            Role = UserRole.User,
            IsActive = false,
            BirthDate = new DateTime(1999, 1, 1)
        };

        var patch = new UserPatchDto
        {
            Forename = "New",
            Email = "new@example.com",
            IsActive = true
        };

        UserPatchApplier.ApplyPatch(existingUser, patch);

        existingUser.Forename.Should().Be("New");
        existingUser.Email.Should().Be("new@example.com");
        existingUser.IsActive.Should().BeTrue();

        existingUser.Surname.Should().Be("Name");
        existingUser.Role.Should().Be(UserRole.User);
        existingUser.BirthDate.Should().Be(new DateTime(1999, 1, 1));
    }
}
