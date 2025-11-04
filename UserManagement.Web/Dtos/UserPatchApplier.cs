using UserManagement.Services.Domain;

namespace UserManagement.Web.Dtos
{
    public static class UserPatchApplier
    {
        public static void ApplyPatch(User existingUser, UserPatchDto patch)
        {
            if (patch.Forename != null) existingUser.Forename = patch.Forename;
            if (patch.Surname != null) existingUser.Surname = patch.Surname;
            if (patch.Email != null) existingUser.Email = patch.Email;
            if (patch.Role.HasValue) existingUser.Role = patch.Role.Value;
            if (patch.IsActive.HasValue) existingUser.IsActive = patch.IsActive.Value;
            if (patch.BirthDate.HasValue) existingUser.BirthDate = patch.BirthDate.Value;
        }

    }
}