# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ASP.NET Core 8 MVC warehouse content management system (products, orders, customers, employees, suppliers) with ASP.NET Identity authentication. The solution has a single project in `Warehouse-CMS/`; there is no test project.

## Commands

All `dotnet` commands run from the `Warehouse-CMS/` project directory (or pass the project path from the repo root):

```powershell
dotnet build Warehouse-CMS.sln          # build (from repo root)
dotnet run --project Warehouse-CMS      # run locally (Development environment)
dotnet watch --project Warehouse-CMS    # run with hot reload
```

EF Core migrations (Microsoft.EntityFrameworkCore.Tools is referenced):

```powershell
dotnet ef migrations add <Name> --project Warehouse-CMS
dotnet ef database update --project Warehouse-CMS
```

Helper scripts in `Warehouse-CMS/` (bash): `migrate-and-seed.sh` (EF update against Production), `reseed.sh` (builds and briefly runs the app to trigger seeding).

## Hosting (Railway)

The app is deployed on Railway (CLI is installed and logged in):

- **Project:** `Warehouse-CMS`, **environment:** `production`, **service:** `Warehouse-CMS`
- **Domain:** https://warehouse-cms-production.up.railway.app
- **Build:** Dockerfile builder ([railway.json](railway.json)). The [Dockerfile](Dockerfile) does **not** build the app — it copies a pre-built `publish_output/` directory into an `aspnet:8.0-alpine` runtime image. So a deploy requires publishing locally first:

```powershell
dotnet publish Warehouse-CMS -c Release -o publish_output /p:ExcludeDevDeps=true
railway up
```

- **Start command:** `dotnet Warehouse-CMS.dll`, restart policy ON_FAILURE (max 10 retries)
- **Database:** Railway Postgres, injected via the `DATABASE_URL` env var (see provider selection below)
- Useful CLI: `railway status`, `railway logs`, `railway up`. Note `publish_output/` is gitignored but must exist locally for the Docker build.
- The port comes from Railway's `PORT` env var (`ASPNETCORE_URLS=http://0.0.0.0:${PORT:-3000}` in the Dockerfile)
- Health check endpoint: `/health` (anonymous)

There is also a legacy GitHub Actions workflow deploying to Azure Web Apps on pushes to the `dbconnection` branch (`.github/workflows/azure-webapps-dotnet-core.yml`).

## Architecture

### Database provider selection (Program.cs)

`ApplicationDbContext` is registered with a provider chosen at startup, in this order:

1. If `DATABASE_URL` is set (Railway) → **PostgreSQL** (Npgsql), URL parsed into a connection string with SSL required
2. Else in Development/Testing → **SQL Server** via `ConnectionStrings:DefaultConnection` (appsettings.Development.json / user secrets)
3. Else → **SQL Server** via `ConnectionStrings:AZURE_SQL_CONNECTIONSTRING`

The single `Migrations/` set is shared across providers — be careful that new migrations work on both Postgres and SQL Server.

### Startup migration and seeding

In non-Development environments, `Database.Migrate()` runs automatically on startup, followed by `SeedDatabase.SeedAsync()` in all environments. Seeding creates employee roles (Admin, Sales, Manager…), mirrors them into Identity roles, and creates an admin user whose password comes from the `ADMIN_PASSWORD` env var (falls back to a default). Seeding failures are logged but don't stop the app.

### Repository pattern

Controllers depend on interfaces in `Repositories/Interfaces/`, implemented by EF classes in `Repositories/Implementation/` (all registered as scoped in Program.cs). `Repositories/Mock/` holds in-memory implementations that are not registered — swap registrations to use them. Business logic that spans repositories lives in `Services/` (e.g. `InventoryService`, `RoleManagementService`).

### Authentication & environments

- ASP.NET Identity with default UI (`Areas/Identity`); Google external login is enabled only when `Authentication:Google:ClientId/ClientSecret` are configured (user secrets locally, env vars in prod)
- Behavior is heavily environment-dependent: Development/Testing relax password rules and cookie security, Staging/Production enforce strict passwords, HTTPS-only cookies, and longer cookie lifetimes. There are separate appsettings files for Development, Staging, Testing, and Production.
- Custom error pages route through `EnvironmentController` (`/Environment/StatusCode`) in non-dev environments

### Secrets

Local secrets live in user secrets, `Warehouse-CMS/.env`, and `Warehouse-CMS/secrets.json` (both gitignored). Never commit these or echo their contents.
