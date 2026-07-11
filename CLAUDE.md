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
- **Build:** Dockerfile builder ([railway.json](railway.json)). The [Dockerfile](Dockerfile) is a self-contained multi-stage build (`sdk:8.0-alpine` restore/publish → `aspnet:8.0-alpine` runtime), so it builds reproducibly from source — no local publish step required. Deploy with:

```powershell
railway up
```

- **Port:** injected by Railway via `PORT` at runtime; the Dockerfile ENTRYPOINT resolves it at container start (`ASPNETCORE_URLS=http://0.0.0.0:${PORT:-3000}`), not at build time.
- **Restart policy:** ON_FAILURE (max 10 retries)
- **Database:** Railway Postgres, injected via the `DATABASE_URL` env var
- **Required env vars in production:** `DATABASE_URL` (Postgres), `ADMIN_PASSWORD` (seeding throws if unset outside Development/Testing), optional `ADMIN_EMAIL`, and `Authentication:Google:ClientId/ClientSecret` for Google login.
- Useful CLI: `railway status`, `railway logs`, `railway up`.
- Health check endpoint: `/health` (anonymous)

## Architecture

### Database provider (Program.cs)

The app runs on **PostgreSQL (Npgsql) only**. `ApplicationDbContext` uses:

1. `DATABASE_URL` (Railway) when set — parsed via `BuildNpgsqlConnectionString` (URL-decodes credentials, SSL required), or
2. `ConnectionStrings:DefaultConnection` for local development (points at local Postgres in appsettings.Development.json).

`EnableRetryOnFailure()` is on for transient-fault resilience. Migrations in `Migrations/` are Postgres-flavored — do not reintroduce SQL Server without regenerating them.

### Startup migration and seeding

`Database.MigrateAsync()` runs on startup in **all** environments (including Development, so a fresh dev database gets its schema), followed by `SeedDatabase.SeedAsync()`. Seeding creates employee roles (Admin, Sales, Manager…), mirrors them into Identity roles, and creates an admin user. The admin password comes from `ADMIN_PASSWORD`; outside Development/Testing, seeding **throws** if it's unset rather than using a default. The seed call in Program.cs is wrapped in a try/catch that logs failures.

### Repository pattern

Controllers depend on interfaces in `Repositories/Interfaces/`, implemented by EF classes in `Repositories/Implementation/` (all registered as scoped in Program.cs). Business logic that spans repositories lives in `Services/` (e.g. `InventoryService`, `RoleManagementService`).

### SPA-style navigation

`_Layout.cshtml` intercepts non-Identity link clicks and form submits via XHR (`X-Requested-With` header) and swaps `#content-container`. Controllers cooperate through `BaseController`: `ViewOrPartial` returns a partial for AJAX requests and a full view otherwise, and `JsonOrRedirect` returns `{ success, redirectUrl }` JSON (which the layout follows) instead of an HTTP redirect. Full-page `Index` views are thin wrappers that render the same partial the AJAX path returns.

### Authentication & environments

- ASP.NET Identity with default UI (`Areas/Identity`); Google external login is enabled only when `Authentication:Google:ClientId/ClientSecret` are configured (user secrets locally, env vars in prod)
- Behavior is heavily environment-dependent: Development/Testing relax password rules and cookie security, Staging/Production enforce strict passwords, HTTPS-only cookies, and longer cookie lifetimes. There are separate appsettings files for Development, Staging, Testing, and Production.
- Custom error pages route through `EnvironmentController` (`/Environment/StatusCode`) in non-dev environments

### Secrets

Local secrets live in user secrets, `Warehouse-CMS/.env`, and `Warehouse-CMS/secrets.json` (both gitignored). Never commit these or echo their contents.
