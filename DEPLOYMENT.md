# Tagalong Deployment Guide

## GitHub Repository
- **URL**: https://github.com/PranavM210106/tagalong
- **Branch**: main

## Test Credentials
> **Note**: Test credentials are seeded via SeedUsers project. Run `dotnet run --project SeedUsers/SeedUsers.csproj` after deployment to create test users. Check SeedUsers/Program.cs for current credentials.

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

## Render Deployment (Backend + Database)

### Option 1: render.yaml (Infrastructure as Code)
1. Go to https://dashboard.render.com
2. Click **New** → **Blueprint**
3. Connect GitHub repo: `PranavM210106/tagalong`
4. Render will detect `render.yaml` and create:
   - Web Service (Docker)
   - PostgreSQL Database (Free tier)
5. Click **Apply**

### Option 2: Manual Setup
1. **Create PostgreSQL Database**:
   - New → PostgreSQL
   - Name: `tagalong-db`
   - Database: `tagalong`
   - User: `tagalong`
   - Plan: Free
   - Copy the **Internal Connection String**

2. **Create Web Service**:
   - New → Web Service
   - Connect GitHub: `PranavM210106/tagalong`
   - Environment: Docker
   - Dockerfile Path: `./Dockerfile`
   - Plan: Free
   - Environment Variables:
     ```
     ASPNETCORE_ENVIRONMENT = Production
     ConnectionStrings__DefaultConnection = <Paste Internal Connection String>
     ASPNETCORE_URLS = http://0.0.0.0:10000
     ```

---

## Post-Deployment Configuration

### 1. Update Vercel with Render Database URL
After Render creates the database, copy the **External Connection String** and update Vercel environment variable `ConnectionStrings__DefaultConnection`.

### 2. Run Database Migrations
The app will auto-migrate on first run. If needed, run manually:
```bash
# On Render shell
dotnet ef database update --project Tagalong.csproj
```

### 3. Seed Test Users
```bash
# On Render shell
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