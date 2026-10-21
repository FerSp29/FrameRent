using FrameRent.Domain.Constants;
using FrameRent.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FrameRent.Infrastructure.Persistence;

public static class DbInitializer
{
    public const string DefaultStaffEmail = "staff@framerent.com";
    public const string DefaultStaffPassword = "StaffPassword123!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Seed Roles
            foreach (var roleName in new[] { Roles.Staff, Roles.Customer })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Default Staff User
            var staffUser = await userManager.FindByEmailAsync(DefaultStaffEmail);
            if (staffUser == null)
            {
                staffUser = new IdentityUser
                {
                    UserName = DefaultStaffEmail,
                    Email = DefaultStaffEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(staffUser, DefaultStaffPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(staffUser, Roles.Staff);
                    logger.LogInformation("Default Staff account seeded: {Email}", DefaultStaffEmail);
                }
            }

            // 2.1 Seed Default Demo Customer User
            const string defaultCustomerEmail = "cliente@framerent.com";
            const string defaultCustomerPassword = "Cliente123!";
            var customerUser = await userManager.FindByEmailAsync(defaultCustomerEmail);
            if (customerUser == null)
            {
                customerUser = new IdentityUser
                {
                    UserName = defaultCustomerEmail,
                    Email = defaultCustomerEmail,
                    EmailConfirmed = true
                };

                var createCustResult = await userManager.CreateAsync(customerUser, defaultCustomerPassword);
                if (createCustResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, Roles.Customer);
                    logger.LogInformation("Default Customer account seeded: {Email}", defaultCustomerEmail);
                }
            }

            // 3. Seed Initial Equipment Catalog if empty
            if (!await context.Equipments.AnyAsync())
            {
                var initialCatalog = new List<Equipment>
                {
                    new(
                        "Sony FX3 Full-Frame Cinema Camera",
                        "Cameras",
                        "Compact 4K 120p full-frame cinema camera with S-Cinetone and dual base ISO.",
                        120.00m,
                        4),
                    new(
                        "Canon RF 24-70mm f/2.8L IS USM",
                        "Lenses",
                        "Professional standard zoom lens with optical image stabilization and constant f/2.8 aperture.",
                        45.00m,
                        6),
                    new(
                        "Aputure Light Storm 300d II LED",
                        "Lighting",
                        "Daylight-balanced 350W COB LED continuous light with Bowens mount and wireless control.",
                        55.00m,
                        5),
                    new(
                        "Sennheiser MKH 416 Shotgun Microphone",
                        "Audio",
                        "Industry-standard supercardioid shotgun condenser microphone for film and broadcast.",
                        35.00m,
                        8),
                    new(
                        "DJI RS 3 Pro Gimbal Stabilizer",
                        "Stabilizers & Support",
                        "Carbon fiber 3-axis motorized gimbal for cinema cameras up to 4.5 kg.",
                        50.00m,
                        5),
                    new(
                        "Sachtler Flowtech 75 Carbon Fiber Tripod",
                        "Tripods",
                        "Quick-release carbon fiber tripod legs with fluid video head for professional camcorders.",
                        40.00m,
                        6)
                };

                await context.Equipments.AddRangeAsync(initialCatalog);
                await context.SaveChangesAsync();
                logger.LogInformation("Initial equipment catalog seeded successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
