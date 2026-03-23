using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Food.Domain.Models;

namespace Food.MVC.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        logger.LogInformation("Starting database seeding");

        await SeedRolesAsync(roleManager, logger);
        await SeedUsersAsync(userManager, logger);
        await SeedPremisesAndInspectionsAsync(context, logger);

        logger.LogInformation("Database seeding completed");
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        string[] roles = { "Admin", "Inspector", "Viewer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Role created: {RoleName}", role);
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var users = new[]
        {
            new { Email = "admin@foodsafety.local", Password = "Admin123!", Role = "Admin" },
            new { Email = "inspector@foodsafety.local", Password = "Inspector123!", Role = "Inspector" },
            new { Email = "viewer@foodsafety.local", Password = "Viewer123!", Role = "Viewer" }
        };

        foreach (var userData in users)
        {
            if (await userManager.FindByEmailAsync(userData.Email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userData.Email,
                    Email = userData.Email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, userData.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, userData.Role);
                    logger.LogInformation("User created: {Email} with role {Role}", userData.Email, userData.Role);
                }
            }
        }
    }

    private static async Task SeedPremisesAndInspectionsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (context.Premises.Any())
        {
            logger.LogInformation("Database already contains data, skipping seed");
            return;
        }

        var restaurantNames = new[]
        {
            "Supermac's", "McDonald's", "Burger King", "KFC",
            "Subway", "Domino's Pizza", "Papa John's Pizza", "Pizza Hut",
            "Four Star Pizza", "Apache Pizza", "Abrakebabra", "Boojum"
        };

        var towns = new[] { "Auckland", "Wellington", "Christchurch" };
        var premisesList = new List<Premises>();

        for (int i = 0; i < restaurantNames.Length; i++)
        {
            premisesList.Add(new Premises
            {
                Name = restaurantNames[i],
                Address = $"{(i + 1) * 10} Main Street",
                Town = towns[i % 3],
                RiskRating = (RiskRating)(i % 3)
            });
        }

        context.Premises.AddRange(premisesList);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} premises", premisesList.Count);

        var random = new Random(42);
        var inspections = new List<Inspection>();

        foreach (var premises in premisesList)
        {
            int inspectionCount = random.Next(1, 4);
            for (int j = 0; j < inspectionCount; j++)
            {
                int daysAgo = random.Next(1, 180);
                int score = random.Next(50, 101);
                
                inspections.Add(new Inspection
                {
                    PremisesId = premises.Id,
                    InspectionDate = DateTime.Now.AddDays(-daysAgo),
                    Score = score,
                    Outcome = score >= 70 ? InspectionOutcome.Pass : InspectionOutcome.Fail,
                    Notes = $"Inspection notes for {premises.Name}"
                });

                if (inspections.Count >= 25) break;
            }
            if (inspections.Count >= 25) break;
        }

        context.Inspections.AddRange(inspections);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} inspections", inspections.Count);

        var followUps = new List<FollowUp>();
        var failedInspections = inspections.Where(i => i.Outcome == InspectionOutcome.Fail).Take(10).ToList();

        for (int i = 0; i < Math.Min(10, failedInspections.Count); i++)
        {
            var inspection = failedInspections[i];
            int dueDaysFromInspection = random.Next(7, 30);
            bool isOverdue = i < 4;
            bool isClosed = i >= 7;

            var dueDate = isOverdue 
                ? inspection.InspectionDate.AddDays(dueDaysFromInspection).AddDays(-60) 
                : inspection.InspectionDate.AddDays(dueDaysFromInspection);

            followUps.Add(new FollowUp
            {
                InspectionId = inspection.Id,
                DueDate = dueDate,
                Status = isClosed ? FollowUpStatus.Closed : FollowUpStatus.Open,
                ClosedDate = isClosed ? dueDate.AddDays(random.Next(1, 5)) : null
            });
        }

        context.FollowUps.AddRange(followUps);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} follow-ups ({OverdueCount} overdue, {ClosedCount} closed)", 
            followUps.Count, 
            followUps.Count(f => f.Status == FollowUpStatus.Open && f.DueDate < DateTime.Now),
            followUps.Count(f => f.Status == FollowUpStatus.Closed));
    }
}
