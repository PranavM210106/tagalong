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
// On Render, Blueprint with fromDatabase injects connection string as env var
// Try multiple sources in order of precedence
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL"); // Render also uses this sometimes

// On Linux (Render), ALWAYS use PostgreSQL. On Windows, use SQL Server only for local LocalDB.
var isLinux = OperatingSystem.IsLinux();
var isLocalDb = !isLinux && !string.IsNullOrEmpty(connectionString) &&
    (connectionString.Contains("localdb", StringComparison.OrdinalIgnoreCase) ||
     connectionString.Contains("mssqllocaldb", StringComparison.OrdinalIgnoreCase) ||
     connectionString.Contains("Data Source=(localdb)", StringComparison.OrdinalIgnoreCase));

// On Linux, validate connection string is PostgreSQL format; if not, try to build from Render PG env vars
if (isLinux && !string.IsNullOrEmpty(connectionString))
{
    // First: explicitly reject SQL Server connection strings
    var isSqlServer = connectionString.Contains("Trusted_Connection", StringComparison.OrdinalIgnoreCase) ||
                      connectionString.Contains("mssqllocaldb", StringComparison.OrdinalIgnoreCase) ||
                      connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase) ||
                      connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase);

    // Check if it's a valid PostgreSQL connection string (key=value or URL format)
    var isPostgres = connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
                     (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) &&
                      connectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase) &&
                      !isSqlServer) ||  // Server=+Database= but NOT SQL Server
                     connectionString.Contains("postgresql://", StringComparison.OrdinalIgnoreCase) ||
                     connectionString.Contains("postgres://", StringComparison.OrdinalIgnoreCase);

    // DEBUG: Log the connection string format (first 50 chars)
    Console.WriteLine($"[DEBUG] Linux detected. Connection string starts with: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
    Console.WriteLine($"[DEBUG] IsSqlServer check: {isSqlServer}");
    Console.WriteLine($"[DEBUG] IsPostgres check result: {isPostgres}");

    if (isSqlServer || !isPostgres)
    {
        Console.WriteLine("[DEBUG] Connection string is SQL Server or not valid PostgreSQL format. Attempting to build from env vars...");

        // Try to build from Render's individual PG environment variables
        var pgHost = Environment.GetEnvironmentVariable("PGHOST");
        var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
        var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
        var pgUser = Environment.GetEnvironmentVariable("PGUSER");
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");

        Console.WriteLine($"[DEBUG] PGHOST={pgHost}, PGPORT={pgPort}, PGDATABASE={pgDatabase}, PGUSER={pgUser}, PGPASSWORD=\"[hidden]\"");

        if (!string.IsNullOrEmpty(pgHost) && !string.IsNullOrEmpty(pgDatabase) &&
            !string.IsNullOrEmpty(pgUser) && !string.IsNullOrEmpty(pgPassword))
        {
            connectionString = $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Require;Trust Server Certificate=true";
            Console.WriteLine("[DEBUG] Built connection string from individual PG env vars");
        }
        else
        {
            // Fallback: try DATABASE_URL (Render sometimes uses this)
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            Console.WriteLine($"[DEBUG] DATABASE_URL={databaseUrl}");
            if (!string.IsNullOrEmpty(databaseUrl))
            {
                connectionString = databaseUrl;
                Console.WriteLine("[DEBUG] Using DATABASE_URL");
            }
            else
            {
                throw new InvalidOperationException(
                    "PostgreSQL connection string not configured. " +
                    "Set ConnectionStrings__DefaultConnection or ensure Render PostgreSQL env vars (PGHOST, PGPORT, PGDATABASE, PGUSER, PGPASSWORD) are available.");
            }
        }
    }
    else
    {
        Console.WriteLine("[DEBUG] Using injected connection string as-is (valid PostgreSQL format)");
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