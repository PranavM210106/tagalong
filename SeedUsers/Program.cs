using System;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tagalong.Data;
using Tagalong.Models.Entities;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = Host.CreateApplicationBuilder(args);

// On Render, Blueprint with fromDatabase injects connection string as env var
// Try multiple sources in order of precedence - ENV VARS FIRST on Linux to override appsettings.json
var isLinux = OperatingSystem.IsLinux();
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? (isLinux ? null : builder.Configuration["ConnectionStrings:DefaultConnection"]);

// On Linux (Render), ALWAYS use PostgreSQL. On Windows, use SQL Server only for local LocalDB.
var isLocalDb = !isLinux && !string.IsNullOrEmpty(connectionString) &&
    (connectionString.Contains("localdb", StringComparison.OrdinalIgnoreCase) ||
     connectionString.Contains("mssqllocaldb", StringComparison.OrdinalIgnoreCase) ||
     connectionString.Contains("Data Source=(localdb)", StringComparison.OrdinalIgnoreCase));

// Convert postgresql:// or postgres:// URL format to key=value format for Npgsql compatibility across all platforms
if (!string.IsNullOrEmpty(connectionString) &&
    (connectionString.Contains("postgresql://", StringComparison.OrdinalIgnoreCase) ||
     connectionString.Contains("postgres://", StringComparison.OrdinalIgnoreCase)))
{
    try
    {
        var uri = new Uri(connectionString);
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

        var query = QueryHelpers.ParseQuery(uri.Query);
        var sslMode = query.ContainsKey("sslmode") ? query["sslmode"].ToString() : "Require";

        connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode={sslMode};Trust Server Certificate=true";
        Console.WriteLine("[DEBUG] Converted postgresql:// URL to key=value format");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DEBUG] Failed to parse postgresql:// URL: {ex.Message}");
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (isLocalDb)
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Apply migrations or ensure schema
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("[INFO] Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARN] MigrateAsync failed ({ex.Message}). Checking schema and falling back...");
        var conn = dbContext.Database.GetDbConnection();
        await conn.OpenAsync();
        bool usersExist = false;
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'AspNetUsers');";
            var res = await cmd.ExecuteScalarAsync();
            usersExist = res != null && Convert.ToBoolean(res);
        }

        if (!usersExist)
        {
            Console.WriteLine("[INFO] AspNetUsers table not found. Cleaning up orphan migration history and ensuring schema...");
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DROP TABLE IF EXISTS \"__EFMigrationsHistory\" CASCADE;";
                await cmd.ExecuteNonQueryAsync();
            }
            await conn.CloseAsync();

            var created = await dbContext.Database.EnsureCreatedAsync();
            Console.WriteLine($"[INFO] Database EnsureCreatedAsync result: {created}");
        }
        else
        {
            Console.WriteLine("[INFO] Database schema already exists.");
            await conn.CloseAsync();
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Ensure roles exist
    string[] roleNames = { "Admin", "Driver", "Rider" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
            Console.WriteLine($"Created role: {roleName}");
        }
    }

    // Create test users
    var testUsers = new[]
    {
        new { Email = "rider@test.com", Password = Environment.GetEnvironmentVariable("DEFAULT_TEST_PASSWORD") ?? "Test123!", FullName = "Test Rider", Role = "Rider" },
        new { Email = "driver@test.com", Password = Environment.GetEnvironmentVariable("DEFAULT_TEST_PASSWORD") ?? "Test123!", FullName = "Test Driver", Role = "Driver" },
        new { Email = "admin@test.com", Password = Environment.GetEnvironmentVariable("DEFAULT_TEST_PASSWORD") ?? "Test123!", FullName = "Test Admin", Role = "Admin" }
    };

    foreach (var u in testUsers)
    {
        var existing = await userManager.FindByEmailAsync(u.Email);
        if (existing == null)
        {
            var user = new ApplicationUser
            {
                UserName = u.Email,
                Email = u.Email,
                FullName = u.FullName,
                PhoneNumber = "+1234567890",
                ProfilePicture = "",
                IsVerified = true,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, u.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, u.Role);
                Console.WriteLine($"Created user: {u.Email} as {u.Role}");
            }
            else
            {
                Console.WriteLine($"Failed to create {u.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // Update password for existing user
            var removeResult = await userManager.RemovePasswordAsync(existing);
            if (removeResult.Succeeded)
            {
                var addResult = await userManager.AddPasswordAsync(existing, u.Password);
                if (addResult.Succeeded)
                {
                    Console.WriteLine($"Updated password for: {u.Email}");
                }
                else
                {
                    Console.WriteLine($"Failed to update password for {u.Email}: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"Failed to remove old password for {u.Email}: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}