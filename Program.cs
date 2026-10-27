using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tagalong.Data;
using Tagalong.Models.Entities;
using Tagalong.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// On Linux (Render), ALWAYS use PostgreSQL. On Windows, use SQL Server only for local LocalDB.
var isLinux = OperatingSystem.IsLinux();
var isLocalDb = !isLinux && !string.IsNullOrEmpty(connectionString) &&
    (connectionString.Contains("localdb", StringComparison.OrdinalIgnoreCase) ||
     connectionString.Contains("mssqllocaldb", StringComparison.OrdinalIgnoreCase) ||
     connectionString.Contains("Data Source=(localdb)", StringComparison.OrdinalIgnoreCase));

// On Linux, validate connection string is PostgreSQL format; if not, try to build from Render PG env vars
if (isLinux && !string.IsNullOrEmpty(connectionString))
{
    // Check if it's a valid PostgreSQL connection string
    var isPostgres = connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
                     connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) &&
                     connectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase) ||
                     connectionString.Contains("postgresql://", StringComparison.OrdinalIgnoreCase) ||
                     connectionString.Contains("postgres://", StringComparison.OrdinalIgnoreCase);

    if (!isPostgres)
    {
        // Try to build from Render's individual PG environment variables
        var pgHost = Environment.GetEnvironmentVariable("PGHOST");
        var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
        var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
        var pgUser = Environment.GetEnvironmentVariable("PGUSER");
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");

        if (!string.IsNullOrEmpty(pgHost) && !string.IsNullOrEmpty(pgDatabase) &&
            !string.IsNullOrEmpty(pgUser) && !string.IsNullOrEmpty(pgPassword))
        {
            connectionString = $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Require;Trust Server Certificate=true";
        }
        else
        {
            // Fallback: try DATABASE_URL (Render sometimes uses this)
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(databaseUrl))
            {
                connectionString = databaseUrl;
            }
            else
            {
                throw new InvalidOperationException(
                    "PostgreSQL connection string not configured. " +
                    "Set ConnectionStrings__DefaultConnection or ensure Render PostgreSQL env vars (PGHOST, PGPORT, PGDATABASE, PGUSER, PGPASSWORD) are available.");
            }
        }
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

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add application services
builder.Services.AddScoped<IDistanceService, DistanceService>();

var app = builder.Build();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames = { "Admin", "Driver", "Rider" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();