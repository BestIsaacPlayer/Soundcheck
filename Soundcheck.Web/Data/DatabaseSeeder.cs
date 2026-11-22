using Microsoft.AspNetCore.Identity;
using Soundcheck.Web.Models;

namespace Soundcheck.Web.Data;

public static class DatabaseSeeder
{
    private const string DemoPassword = "Soundcheck123!";

    private static readonly string[] RoleNames = ["Client", "Consultant", "Admin"];

    private static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string roleName)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return existingUser;
        }

        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName
        };

        var creationResult = await userManager.CreateAsync(newUser, DemoPassword);
        if (!creationResult.Succeeded)
        {
            var errorDescriptions = string.Join("; ", creationResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Seeding user {email} failed: {errorDescriptions}");
        }

        await userManager.AddToRoleAsync(newUser, roleName);
        return newUser;
    }
}
