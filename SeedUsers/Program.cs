using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tagalong.Data;
using Tagalong.Models.Entities;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Create test users
    var testUsers = new[]
    {
        new { Email = "rider@test.com", Password = "Test123!", FullName = "Test Rider", Role = "Rider" },
        new { Email = "driver@test.com", Password = "Test123!", FullName = "Test Driver", Role = "Driver" },
        new { Email = "admin@test.com", Password = "Test123!", FullName = "Test Admin", Role = "Admin" }
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