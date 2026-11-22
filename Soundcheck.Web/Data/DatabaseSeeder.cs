using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Models;

namespace Soundcheck.Web.Data;

public static class DatabaseSeeder
{
    private const string DemoPassword = "Soundcheck123!";

    private static readonly string[] RoleNames = ["Client", "Consultant", "Admin"];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var databaseContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await databaseContext.Database.MigrateAsync();

        foreach (var roleName in RoleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        await CreateUserAsync(userManager, "admin@soundcheck.local", "Admin Soundcheck", "Admin");
        var consultantAnna = await CreateUserAsync(userManager, "anna.nowak@soundcheck.local", "Anna Nowak", "Consultant");
        var consultantTomasz = await CreateUserAsync(userManager, "tomasz.wrobel@soundcheck.local", "Tomasz Wróbel", "Consultant");
        await CreateUserAsync(userManager, "client.one@soundcheck.local", "Jan Kowalski", "Client");
        await CreateUserAsync(userManager, "client.two@soundcheck.local", "Maria Zielińska", "Client");

        if (!await databaseContext.EquipmentCategories.AnyAsync())
        {
            databaseContext.EquipmentCategories.AddRange(
                new EquipmentCategory { Name = "Guitars", Description = "Electric, acoustic and bass guitars" },
                new EquipmentCategory { Name = "Keyboards", Description = "Digital pianos, synthesizers and controllers" },
                new EquipmentCategory { Name = "Studio equipment", Description = "Audio interfaces, microphones and monitors" },
                new EquipmentCategory { Name = "DJ equipment", Description = "Controllers, turntables and mixers" },
                new EquipmentCategory { Name = "Drums", Description = "Acoustic and electronic drum kits" });

            await databaseContext.SaveChangesAsync();
        }

        if (!await databaseContext.ConsultantProfiles.AnyAsync())
        {
            var guitars = await databaseContext.EquipmentCategories.SingleAsync(category => category.Name == "Guitars");
            var keyboards = await databaseContext.EquipmentCategories.SingleAsync(category => category.Name == "Keyboards");
            var studioEquipment = await databaseContext.EquipmentCategories.SingleAsync(category => category.Name == "Studio equipment");

            var annaProfile = new ConsultantProfile
            {
                UserId = consultantAnna.Id,
                Bio = "Session guitarist with a decade of experience choosing instruments for beginners and professionals.",
                HourlyRate = 120m,
                IsActive = true,
                ConsultantCategories =
                {
                    new ConsultantCategory { EquipmentCategory = guitars, YearsOfExperience = 12 },
                    new ConsultantCategory { EquipmentCategory = studioEquipment, YearsOfExperience = 4 }
                },
                AvailabilitySlots = CreateAvailabilitySlots()
            };

            var tomaszProfile = new ConsultantProfile
            {
                UserId = consultantTomasz.Id,
                Bio = "Producer and keyboard player helping with home studio setups and synthesizer purchases.",
                HourlyRate = 150m,
                IsActive = true,
                ConsultantCategories =
                {
                    new ConsultantCategory { EquipmentCategory = keyboards, YearsOfExperience = 15 },
                    new ConsultantCategory { EquipmentCategory = studioEquipment, YearsOfExperience = 10 }
                },
                AvailabilitySlots = CreateAvailabilitySlots()
            };

            databaseContext.ConsultantProfiles.AddRange(annaProfile, tomaszProfile);
            await databaseContext.SaveChangesAsync();
        }
    }

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

    private static List<AvailabilitySlot> CreateAvailabilitySlots()
    {
        var slots = new List<AvailabilitySlot>();
        var firstDay = DateTime.Today.AddDays(1);

        for (var dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            foreach (var startHour in new[] { 10, 14 })
            {
                var startTime = firstDay.AddDays(dayOffset).AddHours(startHour);
                slots.Add(new AvailabilitySlot
                {
                    StartTime = startTime,
                    EndTime = startTime.AddHours(1),
                    IsBooked = false
                });
            }
        }

        return slots;
    }
}
