# Aguas Nico API

.NET 10 Web API for **Aguas Nico**, a water-delivery management system (clients, subscriptions, delivery routes, deliveries, billing, and cash control).

This API is the backend for the [aguas-nico-web](../aguas-nico-web) frontend and is a rewrite of the legacy ASP.NET MVC application. It keeps the same business rules (validations, role permissions, and stock/money calculations) on a thin-controller, service-based architecture.

## Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core with **PostgreSQL** (`Npgsql`)
- JWT bearer authentication
- BCrypt password hashing (`BCrypt.Net-Next`)
- `ClosedXML` for Excel (daily cash-close export)
- SQL files for migrations
- Swagger in development

## Project Structure

```text
AguasNico-Api/
  Controllers/          HTTP endpoints. Controllers are thin and delegate to services.
  DAL/
    DB/                 EF Core DbContext (APIContext) with global soft-delete query filters.
    Migrations/         SQL migration scripts executed at startup.
    MigrationRunner.cs  Runs pending SQL scripts at startup.
  Helpers/              Generic helpers (LocalClock, display-name extensions, etc.).
  Models/
    Constants/          Business enums and constants (roles, states, product types, invoice constants).
    DTO/                Request/response DTOs grouped by module.
  Security/             Role authorization policies and handlers.
  Services/             Business logic, one service per module.
```

## Business Modules

| Module | Controller | Notes |
| --- | --- | --- |
| Inicio (dashboard) | `Home` | Daily totals, sold products, expenses, routes, balance. |
| Clientes | `Client` | CRUD, product/abono associations, invoice data, history, unassigned list. |
| Productos | `Product` | CRUD, per-product stats, client stock. |
| Abonos | `Abono` | Subscription CRUD and renewals (`RenewAll`, `RenewByRoute`). |
| Planillas | `Route` | **Static** templates (one per dealer/day) and **dynamic** daily instances generated from them. |
| Bajadas | `Cart` | Delivery confirmation, states, returns, payments — updates client stock and debt. |
| Repartidores | `Dealer` | Dealers are `User`s with the `Dealer` role; details, sheets, sold products. |
| Gastos | `Expense` | Expense CRUD and date search. |
| Transferencias | `Transfer` | Transfers that adjust client debt. |
| Facturas | `Invoice` | Invoice preview and AFIP-style CSV export. |
| Estadísticas | `Stats` | Annual/monthly profits, products sold, balance by date. |
| Caja | `Caja` | Daily cash-close Excel export (`ClosedXML`). |
| Terceros | `Tercero` | Third-party records. |
| Catálogo | `Catalog` | Enum/combo data for the frontend. |

## Roles

Two roles, enforced with authorization policies (`Models/Constants`, `Security/`):

- `Admin` — full access.
- `Dealer` — own routes/deliveries and the actions allowed during delivery.

## Configuration

Update [appsettings.json](AguasNico-Api/appsettings.json) before deploying.

```json
{
  "ConnectionStrings": {
    "APIContextConnection": "Host=localhost:5432;Username=admin;Password=password;Database=aguasnico;Include Error Detail=true"
  },
  "Database": {
    "RunSqlMigrations": true
  },
  "JWT": {
    "Key": "AguasNicoApiDevelopmentJwtKey-ChangeMe-AtLeast32Chars",
    "Issuer": "AguasNico-Api",
    "Audience": "AguasNico-Api"
  }
}
```

Do not keep the default JWT key in production. The initial admin user is seeded by `02_SeedUser.sql`; rotate its password before deploying anywhere non-local.

## First Run

A local PostgreSQL instance is provided via Docker Compose (`docker-compose.yml`):

```powershell
docker compose up -d
dotnet build AguasNico-Api.slnx
dotnet run --project AguasNico-Api/AguasNico-Api.csproj
```

On startup, the app will:

- Ensure the `_Migrations` table exists.
- Run pending SQL scripts from `AguasNico-Api/DAL/Migrations` (includes role and admin-user seeding).

Swagger is available in development.

## Auth

JWT-based. There is no public registration; users are provisioned by SQL seed. Login:

```http
POST /api/Auth/Login
Content-Type: application/json

{
  "email": "admin@localhost",
  "password": "Password1!"
}
```

Use the returned JWT as a bearer token:

```http
Authorization: Bearer <token>
```

Passwords are hashed with BCrypt. Tokens are valid for 30 days.

## Migrations

Migrations are plain SQL scripts under `AguasNico-Api/DAL/Migrations`, executed in filename order:

```text
01_Initial.sql
02_SeedUser.sql
03_CreateTerceros.sql
```

Each executed filename is recorded in `_Migrations`, so each script runs once. Add new features as the next numbered script.

## Development Notes

- Controllers stay thin and delegate to services; no service interfaces.
- One service per business module, registered in `Services/ServiceContainer.cs`.
- Use SQL scripts for database migrations.
- Use transactions when multiple writes must succeed or fail together (deliveries, renewals, route/cart deletion).
- Soft deletes are handled with `DeletedAt` and global EF query filters in `APIContext`.
- Keep reusable constants under `Models/Constants` and DTOs under `Models/DTO`.

## Build Verification

```powershell
dotnet build AguasNico-Api.slnx
```
