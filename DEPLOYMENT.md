# Tagalong Deployment Guide

## GitHub Repository
- **URL**: https://github.com/PranavM210106/tagalong
- **Branch**: main

## Test Credentials (Seeded Automatically)
The application automatically creates the database schema, roles, and default test accounts on startup:

| Role | Email | Password |
|------|-------|----------|
| **Rider** | `rider@test.com` | `Test123!` |
| **Driver** | `driver@test.com` | `Test123!` |
| **Admin** | `admin@test.com` | `Test123!` |

> **Note**: You can customize these credentials in Render by setting environment variables (`SEED_RIDER_EMAIL`, `SEED_RIDER_PASSWORD`, `SEED_DRIVER_EMAIL`, `SEED_DRIVER_PASSWORD`, `SEED_ADMIN_EMAIL`, `SEED_ADMIN_PASSWORD`).

---

## Vercel Deployment (Frontend)

### Option 1: Vercel Dashboard (Recommended)
1. Go to https://vercel.com/new
2. Import Git Repository: `PranavM210106/tagalong`
3. Framework Preset: **Other** (or .NET)
4. Build Command: `dotnet publish Tagalong.csproj -c Release -o ./output`
5. Output Directory: `./output`
6. Install Command: `dotnet restore`
7. Add Environment Variables:
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `ConnectionStrings__DefaultConnection` = `<Your Render PostgreSQL connection string>`
   - `ASPNETCORE_URLS` = `http://0.0.0.0:3000`
8. Click Deploy

### Option 2: Vercel CLI
```bash
vercel login
cd D:\Tagalong
vercel --prod
```

---

## Render Deployment (Web Service + PostgreSQL)

### Connecting to Your Existing Render PostgreSQL Database
Your shared database credentials:
- **Database Name:** `tagalong_db_la01`
- **Region:** `oregon`
- **External Connection URL:**
  `postgresql://tagalong_db_la01_user:dhdIzeHj5eWvniPSVxxC0ByQsINXbabZ@dpg-dbgh1b4s728c739p6tlg-a.oregon-postgres.render.com/tagalong_db_la01?sslmode=require`

---

### Step-by-Step: Deploy to Render in 3 Minutes

#### Method 1: Web Service Setup (Recommended & Simplest)
1. Log into your [Render Dashboard](https://dashboard.render.com).
2. Click **New +** (top right) ➔ **Web Service**.
3. Select **Build and deploy from a Git repository** and connect `tagalong`.
4. Configure the settings:
   * **Name:** `tagalong` (or any name you choose)
   * **Region:** `Oregon (US West)` *(matches your database)*
   * **Runtime / Environment:** `Docker` *(Render auto-detects `Dockerfile`)*
   * **Instance Type:** `Free`
5. Scroll down to **Environment Variables** and add:
   | Key | Value |
   | :--- | :--- |
   | `ASPNETCORE_ENVIRONMENT` | `Production` |
   | `ConnectionStrings__DefaultConnection` | `postgresql://tagalong_db_la01_user:dhdIzeHj5eWvniPSVxxC0ByQsINXbabZ@dpg-dbgh1b4s728c739p6tlg-a.oregon-postgres.render.com/tagalong_db_la01?sslmode=require` |
   | `PORT` | `8080` |
   | `ASPNETCORE_URLS` | `http://0.0.0.0:8080` |
6. Click **Create Web Service**.

#### Method 2: Blueprint (`render.yaml`)
1. In Render Dashboard, click **New +** ➔ **Blueprint**.
2. Connect your `tagalong` Git repository.
3. Render reads `render.yaml` which automatically configures the web service and the connection string.
4. Click **Apply**.

---

### What Happens Automatically on First Launch
When Render builds and starts the container:
1. **Database Schema Creation:** `Program.cs` automatically detects the PostgreSQL database and applies all EF Core migrations (`MigrateAsync()`).
2. **Seed Roles & Accounts:** It automatically seeds the standard mobility roles (`Admin`, `Driver`, `Rider`) and default test accounts:
   * **Rider:** `rider@test.com` / `Test123!`
   * **Driver:** `driver@test.com` / `Test123!`
   * **Admin:** `admin@test.com` / `Test123!`
3. **Health Check:** Render pings `/health` at port `8080` and turns the service **Live**.

---

## Post-Deployment Configuration

### 1. Update Vercel with Render Database URL
After Render creates the database, copy the **External Connection String** and update Vercel environment variable `ConnectionStrings__DefaultConnection`.

### 2. Database Migrations & Test User Seeding
The application automatically applies all EF Core migrations and seeds the test users on application startup whenever the Docker container launches on Render. No manual shell commands are needed.

If you ever need to seed locally for development:
```bash
dotnet run --project SeedUsers/SeedUsers.csproj
```

### 4. Verify Deployment
- **Vercel URL**: https://tagalong.vercel.app (or your custom domain)
- **Render URL**: https://tagalong.onrender.com
- Test login with seeded test users

---

## Architecture Notes

### Production Stack
- **Frontend**: Vercel (static hosting + edge functions)
- **Backend**: Render (Docker container)
- **Database**: Render PostgreSQL (Free tier)
- **Authentication**: ASP.NET Core Identity with cookies
- **Session**: In-memory (configure Redis for scale)

### Environment Variables Reference
| Variable | Vercel | Render | Description |
|----------|--------|--------|-------------|
| ASPNETCORE_ENVIRONMENT | ✓ | ✓ | Production |
| ConnectionStrings__DefaultConnection | ✓ | ✓ | PostgreSQL connection string |
| ASPNETCORE_URLS | ✓ | ✓ | Port binding (3000 Vercel, 10000 Render) |

---

## Troubleshooting

### Common Issues
1. **CSS not loading**: Ensure `app.UseStaticFiles()` is in Program.cs (already done)
2. **Login not persisting**: Verify `app.UseAuthentication()` and `app.UseAuthorization()` order
3. **Database connection failed**: Check firewall rules on Render PostgreSQL
4. **Migrations not applied**: Run `dotnet ef database update` on Render shell

### Health Checks
- Render: Auto-configured via Dockerfile
- Vercel: Automatic

---

## Local Development
```bash
# Restore dependencies
dotnet restore

# Run database migrations
dotnet ef database update

# Seed test users
dotnet run --project SeedUsers/SeedUsers.csproj

# Run application
dotnet run --project Tagalong.csproj
```
Access at: http://localhost:5019