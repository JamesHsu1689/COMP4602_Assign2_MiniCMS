using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniCMS.Web.Models;

namespace MiniCMS.Web.Data;

public static class DbSeeder
{
    private const string AdminRoleName = "Admin";
    private const string WriterRoleName = "Writer";

    private const string AdminEmail = "a@a.a";
    private const string AdminPassword = "P@$$w0rd";

    private const string WriterEmail1 = "w@w.w";
    private const string WriterEmail2 = "x@x.x";
    private const string WriterPassword = "P@$$w0rd";

    public static async Task SeedAsync(IServiceProvider appServices)
    {
        using var scope = appServices.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        try
        {
            // Ensure DB + migrations are applied (helps on fresh runs / deleted DB)
            var db = services.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();

            await SeedAdminRoleAndUserAsync(services, logger);
            await SeedWriterRoleAndUsersAsync(services, logger);
            await SeedArticlesAsync(services, logger);

            logger.LogInformation("Seeding completed.");
        }
        catch (Exception ex)
        {
            // Must not hard-crash the app due to a seed issue
            logger.LogError(ex, "Seeding failed (app will continue running).");
            Console.WriteLine($"Seeding failed (app will continue running): {ex.Message}");
        }
    }

    private static async Task SeedAdminRoleAndUserAsync(IServiceProvider services, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // 1) Role
        if (!await roleManager.RoleExistsAsync(AdminRoleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(AdminRoleName));
            if (roleResult.Succeeded)
            {
                logger.LogInformation("Seeded role: {Role}", AdminRoleName);
                Console.WriteLine($"Seeded role: {AdminRoleName}");
            }
            else
            {
                logger.LogWarning("Failed to create role {Role}: {Errors}",
                    AdminRoleName, string.Join("; ", roleResult.Errors.Select(e => e.Description)));
                Console.WriteLine($"Failed to create role {AdminRoleName}: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        // 2) User
        var user = await userManager.FindByEmailAsync(AdminEmail);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true
            };

            var userResult = await userManager.CreateAsync(user, AdminPassword);

            if (userResult.Succeeded)
            {
                logger.LogInformation("Seeded admin user: {Email}", AdminEmail);
                Console.WriteLine($"Seeded admin user: {AdminEmail}");
            }
            else
            {
                logger.LogWarning("Failed to create admin user {Email}: {Errors}",
                    AdminEmail, string.Join("; ", userResult.Errors.Select(e => e.Description)));
                Console.WriteLine($"Failed to create admin user {AdminEmail}: {string.Join("; ", userResult.Errors.Select(e => e.Description))}");

                // If user creation failed, don't try to add to role
                return;
            }
        }

        // 3) Ensure role assignment
        if (!await userManager.IsInRoleAsync(user, AdminRoleName))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(user, AdminRoleName);
            if (addToRoleResult.Succeeded)
            {
                logger.LogInformation("Assigned {Email} to role {Role}", AdminEmail, AdminRoleName);
                Console.WriteLine($"Assigned {AdminEmail} to role {AdminRoleName}");
            }
            else
            {
                logger.LogWarning("Failed to assign {Email} to role {Role}: {Errors}",
                    AdminEmail, AdminRoleName, string.Join("; ", addToRoleResult.Errors.Select(e => e.Description)));
                Console.WriteLine($"Failed to assign {AdminEmail} to role {AdminRoleName}: {string.Join("; ", addToRoleResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task SeedWriterRoleAndUsersAsync(IServiceProvider services, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // 1) Role
        if (!await roleManager.RoleExistsAsync(WriterRoleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(WriterRoleName));
            if (roleResult.Succeeded)
            {
                logger.LogInformation("Seeded role: {Role}", WriterRoleName);
                Console.WriteLine($"Seeded role: {WriterRoleName}");
            }
            else
            {
                logger.LogWarning("Failed to create role {Role}: {Errors}",
                    WriterRoleName, string.Join("; ", roleResult.Errors.Select(e => e.Description)));
                Console.WriteLine($"Failed to create role {WriterRoleName}: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        await SeedWriterUserAsync(userManager, logger, WriterEmail1);
        await SeedWriterUserAsync(userManager, logger, WriterEmail2);
    }

    private static async Task SeedWriterUserAsync(
        UserManager<IdentityUser> userManager,
        ILogger logger,
        string email)
    {
        // 1) User
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var userResult = await userManager.CreateAsync(user, WriterPassword);

            if (userResult.Succeeded)
            {
                logger.LogInformation("Seeded writer user: {Email}", email);
                Console.WriteLine($"Seeded writer user: {email}");
            }
            else
            {
                logger.LogWarning("Failed to create writer user {Email}: {Errors}",
                    email, string.Join("; ", userResult.Errors.Select(e => e.Description)));
                Console.WriteLine($"Failed to create writer user {email}: {string.Join("; ", userResult.Errors.Select(e => e.Description))}");

                // If user creation failed, don't try to add to role
                return;
            }
        }

        // 2) Ensure role assignment
        if (!await userManager.IsInRoleAsync(user, WriterRoleName))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(user, WriterRoleName);
            if (addToRoleResult.Succeeded)
            {
                logger.LogInformation("Assigned {Email} to role {Role}", email, WriterRoleName);
                Console.WriteLine($"Assigned {email} to role {WriterRoleName}");
            }
            else
            {
                logger.LogWarning("Failed to assign {Email} to role {Role}: {Errors}",
                    email, WriterRoleName, string.Join("; ", addToRoleResult.Errors.Select(e => e.Description)));
                Console.WriteLine($"Failed to assign {email} to role {WriterRoleName}: {string.Join("; ", addToRoleResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task SeedArticlesAsync(IServiceProvider services, ILogger logger)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser is null)
        {
            logger.LogWarning("Admin user {Email} was not found. Skipping article seeding.", AdminEmail);
            Console.WriteLine($"Admin user {AdminEmail} was not found. Skipping article seeding.");
            return;
        }

        // Backfill ownership for any existing seeded articles that do not yet have a UserId
        var articlesMissingOwner = await db.Articles
            .Where(a => a.UserId == null || a.UserId == string.Empty)
            .ToListAsync();

        if (articlesMissingOwner.Count > 0)
        {
            foreach (var article in articlesMissingOwner)
            {
                article.UserId = adminUser.Id;
            }

            await db.SaveChangesAsync();

            logger.LogInformation("Backfilled owner for {Count} existing article(s).", articlesMissingOwner.Count);
            Console.WriteLine($"Backfilled owner for {articlesMissingOwner.Count} existing article(s).");
        }

        // Only seed if fewer than 6 exist (idempotent)
        var currentCount = await db.Articles.CountAsync();
        if (currentCount >= 6)
        {
            logger.LogInformation("Articles already seeded (count = {Count}).", currentCount);
            Console.WriteLine($"Articles already seeded (count = {currentCount}).");
            return;
        }

        // A simple deterministic list (we'll add until we reach 6, skipping duplicates by Title)
        var candidates = new (string Title, string Content)[]
        {
            ("Welcome to MiniCMS", "This is a seeded article to verify startup seeding works."),
            ("About This Project", "MiniCMS is a small CMS backend built for COMP 4602."),
            ("Admin Account Seeded", "An Admin role and user are created at startup for development."),
            ("SQLite + EF Core", "This project uses SQLite with EF Core migrations."),
            ("Articles Are Seeded", "At least six articles should exist after the first run."),
            ("Next Milestone Preview", "Role-based restriction will come later; this milestone seeds only.")
        };

        // Existing titles to avoid duplicates if you re-run after partial data exists
        var existingTitles = await db.Articles
            .Select(a => a.Title)
            .ToListAsync();

        var targetToAdd = 6 - currentCount;
        var added = 0;

        foreach (var (title, content) in candidates)
        {
            if (added >= targetToAdd) break;
            if (existingTitles.Contains(title)) continue;

            // IMPORTANT: rely on SaveChanges overrides for timestamps
            db.Articles.Add(new Article
            {
                Title = title,
                Content = content,
                UserId = adminUser.Id
            });

            added++;
        }

        if (added > 0)
        {
            await db.SaveChangesAsync();
        }

        var finalCount = await db.Articles.CountAsync();
        logger.LogInformation("Seeded {Added} article(s). Total articles now: {Total}", added, finalCount);
        Console.WriteLine($"Seeded {added} article(s). Total articles now: {finalCount}");
    }
}