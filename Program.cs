using System;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http;
using Tagalong.Data;
using Tagalong.Models.Entities;
using Tagalong.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
// On Render, Blueprint with fromDatabase injects connection string as env var
// Try multiple sources in order of precedence - ENV VARS FIRST on Linux to override appsettings.json
var isLinux = OperatingSystem.IsLinux();
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? (isLinux ? null : builder.Configuration.GetConnectionString("DefaultConnection"));
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

        // Convert postgresql:// URL format to key=value format for Npgsql compatibility
        if (connectionString.Contains("postgresql://", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(connectionString);
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port : 5432;
                var database = uri.AbsolutePath.TrimStart('/');
                var userInfo = uri.UserInfo.Split(':');
                var username = userInfo.Length > 0 ? userInfo[0] : "";
                var password = userInfo.Length > 1 ? userInfo[1] : "";

                // Parse query parameters
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
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        });
    }

    // Enable detailed logging in development or when DEBUG env var is set
    if (!isLinux || Environment.GetEnvironmentVariable("DEBUG") == "true")
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure Identity cookie for Render/Vercel proxy (HTTPS at edge, HTTP to container)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
});

// Configure cookie policy for cross-site cookies
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.Always;
});

// Add application services
builder.Services.AddScoped<IDistanceService, DistanceService>();

var app = builder.Build();

// Apply database migrations on startup (in case preDeployCommand didn't run)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        Console.WriteLine("[INFO] Checking database connection...");
        var canConnect = await dbContext.Database.CanConnectAsync();
        Console.WriteLine($"[INFO] Database connection: {(canConnect ? "OK" : "FAILED")}");

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            Console.WriteLine($"[INFO] Applying {pendingMigrations.Count()} pending migrations...");
            foreach (var m in pendingMigrations)
            {
                Console.WriteLine($"[INFO]   - {m}");
            }
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("[INFO] Migrations applied successfully");
        }
        else
        {
            Console.WriteLine("[INFO] No pending migrations");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Migration failed: {ex.Message}");
        Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
    }
}

// Health check endpoint for Render - MUST be before UseHttpsRedirection for TCP health checks
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// One-time database initialization (seed only - migrations run via preDeployCommand and startup)
// Wrapped in try-catch to avoid crashing app startup if seeding fails
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed roles
        string[] roleNames = { "Admin", "Driver", "Rider" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    Console.WriteLine($"[ERROR] Failed to create role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        // Seed test users from environment variables (optional, defaults for dev)
        var testUsers = new[]
        {
            new {
                Email = Environment.GetEnvironmentVariable("SEED_RIDER_EMAIL") ?? "rider@test.com",
                Password = Environment.GetEnvironmentVariable("SEED_RIDER_PASSWORD") ?? "Test123!",
                FullName = "Test Rider", Role = "Rider"
            },
            new {
                Email = Environment.GetEnvironmentVariable("SEED_DRIVER_EMAIL") ?? "driver@test.com",
                Password = Environment.GetEnvironmentVariable("SEED_DRIVER_PASSWORD") ?? "Test123!",
                FullName = "Test Driver", Role = "Driver"
            },
            new {
                Email = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL") ?? "admin@test.com",
                Password = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD") ?? "Test123!",
                FullName = "Test Admin", Role = "Admin"
            }
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
                    Console.WriteLine($"[INFO] Created user: {u.Email}");
                }
                else
                {
                    Console.WriteLine($"[ERROR] Failed to create user {u.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Update password for existing user
                var removeResult = await userManager.RemovePasswordAsync(existing);
                if (removeResult.Succeeded)
                {
                    var addResult = await userManager.AddPasswordAsync(existing, u.Password);
                    if (!addResult.Succeeded)
                    {
                        Console.WriteLine($"[ERROR] Failed to update password for {u.Email}: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine($"[ERROR] Failed to remove old password for {u.Email}: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] Database seeding failed: {ex.Message}");
    Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
}

// Configure the HTTP request pipeline.
// Trust Render/Vercel proxy headers for HTTPS detection - MUST be first
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor,
    // Trust all proxies (edge IPs change) - KnownNetworks/KnownProxies default to empty
    ForwardLimit = null
});

// Safety net: Force HTTPS scheme when behind proxy
app.Use((context, next) =>
{
    if (context.Request.Headers.ContainsKey("X-Forwarded-Proto"))
    {
        var proto = context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
        if (!string.IsNullOrEmpty(proto) && proto.Contains("https", StringComparison.OrdinalIgnoreCase))
        {
            context.Request.Scheme = "https";
        }
    }
    return next();
});

// Global exception logging middleware (before UseExceptionHandler)
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[UNHANDLED EXCEPTION] {ex.Message}");
        Console.WriteLine($"[UNHANDLED EXCEPTION] Stack: {ex.StackTrace}");
        throw; // Re-throw to let UseExceptionHandler handle it
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Disable HTTPS redirection - Vercel handles SSL termination
// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Read PORT from environment (Vercel sets this at runtime)
var serverPort = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Clear();
app.Urls.Add($"http://0.0.0.0:{serverPort}");

app.Run();