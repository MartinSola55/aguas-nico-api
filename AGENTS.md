# AGENTS.md

Guidance for future work in this repository.

## Source Of Truth

- This repository is the backend API for **Aguas Nico**, a water-delivery management system (clients, subscriptions/abonos, delivery routes/planillas, deliveries/bajadas, billing, and cash control). The frontend lives separately in `aguas-nico-web`.
- This API is a rewrite of a legacy ASP.NET MVC application and must keep the same business rules (validations, role permissions, stock/money calculations).
- Prefer patterns already present in this codebase over inventing new structure.

## Project Shape

- Target framework: `.NET 10`, `net10.0`.
- Solution file: `AguasNico-Api.slnx`.
- Main project folder: `AguasNico-Api/`.
- Root namespace: `AguasNico_Api`.
- `ImplicitUsings` is enabled; do not add `using` directives for namespaces already covered by implicit usings.
- Single API project unless explicitly requested otherwise.
- Keep root-level documentation in `README.md`.

## Folder Structure

Use this structure:

```text
AguasNico-Api/
  Controllers/
  DAL/
    DB/
    Migrations/
    MigrationRunner.cs
  Helpers/
    Interceptors/
  Models/
    Constants/
    DTO/
  Security/
  Services/
```

## Formatting Style

- C# uses **file-scoped namespaces**:

```csharp
namespace AguasNico_Api.Services;

public class ExampleService
{
}
```

- Use 4 spaces for C# indentation.
- Keep explicit `using` directives (beyond implicit usings) at the top of files.
- Keep classes in one file named after the class (DTOs are the exception — see below).
- Use **primary constructors**; every service and controller uses them.
- Store constructor parameters in private readonly fields when used repeatedly (e.g. `private readonly APIContext _db = context;`).
- Prefer collection expressions (`[...]`) instead of `.ToList()` / `new List<>()` when valid.
- Use `#region` sections only where a service already follows that style (e.g. `RouteService`); do not force regions elsewhere.
- Project XML uses 2-space indentation.
- JSON config uses 2-space indentation.
- SQL scripts use uppercase SQL keywords and 4-space indentation inside `BEGIN`/`DO $$` blocks.

## Controllers

- Controllers should only be a bypass/delegation layer to services.
- Do not put business logic in controllers; delegate to the matching service, usually with expression-bodied members:

```csharp
[HttpGet]
public async Task<BaseResponse<GetProfileResponse>> GetProfile([FromQuery] GetProfileRequest rq) => await _userService.GetProfile(rq);
```

- Controllers return `BaseResponse` or `BaseResponse<T>` directly.
- Protected controllers inherit from `BaseController`.
- `BaseController` carries `[ApiController]`, `[Route("api/[controller]/[action]")]`, and `[Authorize]`.
- `AuthController` does **not** inherit from `BaseController` (login/logout must be reachable without auth); it declares `[ApiController]` and the route attribute itself.
- Use `[Authorize(Policy = Policies.Admin)]` / `[Authorize(Policy = Policies.Dealer)]` for role-restricted actions, applied at the controller or action level.
- Use `[FromBody]` for POST payloads and `[FromQuery]` for GET requests.

## Services

- Put business logic in services under `Services/`, one service per business module.
- Register services in `ServiceContainer.AddServices` as `AddScoped`.
- Do not create or implement service interfaces.
- Keep service names concrete: `AuthService`, `UserService`, `RouteService`, `CartService`, `CajaService`, etc.
- Use EF Core directly through `APIContext` (stored as `_db`).
- Read the current user from `TokenService` (`tokenService.GetToken()`), exposed as a `Token` field, and check `_token.UserId` / `_token.Role` for authorization decisions inside services.
- Use `try/catch` around persistence and external work where failure should return a `BaseResponse` error (`Messages.Error.Exception()`).
- Return user-facing errors through `Messages` and `rs.SetError(...)`.
- Do not throw for expected validation failures — return an error response instead.

## Interfaces

- No custom service interfaces should be created or implemented.
- Do not add `IUserService`, repository interfaces, or similar abstractions.
- Use framework-provided types only where required by package/framework registration.

## Auth And Roles

- Use custom `User` and `Role` entities. Do not switch to ASP.NET Identity unless explicitly requested.
- There is no public registration; users are provisioned via SQL seed scripts.
- Passwords are hashed with `BCrypt.Net-Next` (`AuthService.HashPassword` / `ValidatePassword`).
- JWT bearer auth is configured in `Program.cs` using:
  - `Jwt:Issuer`
  - `Jwt:Audience`
  - `Jwt:Key`
- Tokens are minted in `TokenService`/`AuthService`; JWT claims include the user id, name, email, and `ClaimTypes.Role`.
- Role and policy names live in `Models/Constants/BusinessConstants.cs`:
  - `Roles.Admin` = `"ADMIN"`, `Roles.Dealer` = `"DEALER"` (`Roles.GetRoles()` returns the list).
  - `Policies.Admin` = `"Admin"`, `Policies.Dealer` = `"Dealer"`.
- Policies are configured in `Program.cs` with `AddAuthorizationBuilder().AddPolicy(...)`:
  - `Policies.Admin` requires the `Admin` role.
  - `Policies.Dealer` requires `Admin` or `Dealer`.
- `Security/` also contains `AuthorizeRolesAttribute` + `RolesHandler`; the active authorization path is the policy-based `[Authorize(Policy = ...)]` attributes above.

## Responses And DTOs

- Use `BaseResponse` and `BaseResponse<T>` from `Models/DTO/BaseResponse.cs`.
  - `BaseResponse<T>` exposes `Data`, `Message`, `Error`, computed `Success`, `SetError(message, code = 400)`, and `Attach(...)` to propagate a nested response's error.
  - Start handlers with `var rs = new BaseResponse<T>();`, return `rs.SetError(...)` on failure, set `rs.Data`/`rs.Message` on success, and `return rs;`.
- Request/response DTOs live under `Models/DTO/<Module>/` (e.g. `Clients`, `Routes`, `Carts`, `Stats`, `Users`, `Auth`).
- Name DTO files by **operation + module**, and place the request and response classes together in that file:
  - `CreateClient.cs` → `CreateClientRequest`, `CreateClientResponse`
  - `GetAllClients.cs` → `GetAllClientsRequest`, `GetAllClientsResponse`
  - `GetClient.cs`, `UpdateClient.cs`, `DeleteClient.cs`, `UpdatePassword.cs`, etc.
- Shared/nested DTO items live in `Models/DTO/Common/` (e.g. `ProductItem`, `PaymentAmountItem`, `ClientSummaryItem`) or as `*Item` files inside the module folder.
- Combo/lookup responses use `ComboResponse` from `Models/DTO/ComboResponse.cs`.
- Pagination is not used in this project; do not add it unless explicitly requested.

## Models And Data

- Soft-deletable entities inherit from `AuditableEntity` (`Models/AuditableEntity.cs`), which has `CreatedAt`, `UpdatedAt`, `DeletedAt`.
- Soft-delete filtering is done with global query filters registered per entity in `APIContext.OnModelCreating` (`HasQueryFilter(e => e.DeletedAt == null)`); add a filter there for every new soft-deletable entity.
- Composite keys are configured in `OnModelCreating` via `HasKey(...)`.
- `DbSet` property names follow the existing (mixed) style — match neighbours: singular for `User`, `Role`, `Migration`; plural for `Abonos`, `AbonoProducts`, etc.
- Use `[Key]` and `[ForeignKey]` attributes where the current style does.
- All `DateTime` columns are mapped to `timestamp with time zone` (handled globally in `OnModelCreating`).
- Use `LocalClock.Now` / `LocalClock.Today` (which resolve to `DateTime.UtcNow`) for dates instead of calling `DateTime.UtcNow` directly.
- Use `EnumExtensions.GetDisplayName()` to resolve an enum's `[Display(Name = ...)]` value.

## Constants

- Put reusable constants and business enums in `Models/Constants/BusinessConstants.cs` (invoice constants, `Roles`, `Policies`, `PaymentMethodCodes`, and domain enums like `State`, `ProductType`, `Day`, `InvoiceType`, `TaxCondition`).
- Prefer `static` classes for constant groups.
- Do not hard-code repeated status strings, role names, or payment codes in services — reference the constants.
- Enum display labels use `[Display(Name = "...")]` with proper Spanish text.

## User-Facing Messages

- Keep reusable Spanish messages in `Models/Constants/Messages.cs`, grouped as `Messages.Error.*` and `Messages.CRUD.*` static methods.
- Use proper Spanish accents and punctuation. Do not replace Spanish punctuation with ASCII-only text unless explicitly requested.
- Reuse existing helpers (`Messages.Error.EntityNotFound("Usuario")`, `Messages.Error.Unauthorized()`, `Messages.CRUD.EntityUpdated("Repartidor")`, etc.) rather than inlining literal strings.
- Message construction toggles gender with a `femine` flag, consistent with the existing pattern:

```csharp
public static string EntityCreated(string entityName, bool femine = false) => entityName + " cread" + (femine ? "a" : "o") + " correctamente.";
```

## Service / CRUD Pattern

- CRUD-style services generally expose combinations of: `GetCombo`, `GetAll`, `GetOne`/`Get<Entity>`, `Create`, `Update`, `Delete`, plus feature-specific methods.
- Use soft delete by setting `DeletedAt = LocalClock.Now`.
- Set `UpdatedAt = LocalClock.Now` on updates.
- Use `AsNoTracking()` on read-only queries and single-item projections.
- Project to DTOs inside the query with `.Select(...)` rather than materializing full entities when only a projection is needed.
- Apply filtering before ordering.
- Perform authorization checks early (`if (_token.Role != Roles.Admin) return rs.SetError(Messages.Error.Unauthorized(), 403);`).

## Database And Migrations

- Keep all migrations as SQL scripts. Do not add EF-generated migration `.cs` files, and do not use `Database.Migrate()` or EF migrations unless explicitly requested.
- SQL scripts live under `AguasNico-Api/DAL/Migrations/`, ordered with numeric prefixes:
  - `01_Initial.sql`
  - `02_SeedUser.sql`
  - `03_CreateTerceros.sql`
  - `04_AddPaymentMethodCode.sql`
- Add new schema/data changes as the next numbered script.
- SQL scripts are copied to output via the `.csproj` `Content` rule (`DAL\Migrations\**\*.sql`).
- `MigrationRunner` runs pending SQL scripts at startup when `Database:RunSqlMigrations` is `true` (default), each inside a transaction.
- Executed script filenames are recorded in the `_Migrations` table to prevent reruns.
- All data seeding (roles, initial admin user, lookup data, etc.) must be done via SQL scripts. Do not seed data from C# code.
- Target database is **PostgreSQL** (`Npgsql`); use PostgreSQL SQL syntax (quoted identifiers, `timestamp with time zone`, `now()`, `GENERATED BY DEFAULT AS IDENTITY`).

## Interceptors

- EF Core `SaveChangesInterceptor` / interceptor implementations live under `AguasNico-Api/Helpers/Interceptors/`.
- Interceptors are registered through `InterceptorsContainer.AddInterceptors(services)` (DI registration) and applied via `InterceptorsContainer.ConfigureInterceptors(serviceProvider, options)` inside the `AddDbContext` configuration in `Program.cs`.
- When adding a new interceptor, register it in `AddInterceptors` and add it to the `options.AddInterceptors(...)` call in `ConfigureInterceptors`. Do not override `OnConfiguring` on the `DbContext` to register interceptors.
- `DateTimeUtcKindInterceptor` normalizes `DateTime` kinds for the PostgreSQL `timestamp with time zone` mapping.

## Transactions

- Only use transactions when multiple writes must succeed or fail together (e.g. deliveries/bajadas, abono renewals, route/cart deletion).
- Do not wrap read-only operations in transactions.
- Do not add transactions for simple single-entity saves unless there is a real consistency requirement.

## Excel Export

- Use `ClosedXML` (`XLWorkbook`) for spreadsheet generation; the daily cash-close export lives in `CajaService`.
- Return the generated file bytes through the appropriate DTO/response rather than writing to disk.

## Configuration

- Main config is `AguasNico-Api/appsettings.json`; development overrides go in `AguasNico-Api/appsettings.Development.json`.
- Key sections: `ConnectionStrings:APIContextConnection`, `Database:RunSqlMigrations`, and `Jwt` (`Key`, `Issuer`, `Audience`).
- Do not commit real secrets; keep placeholder/dev values and rotate the JWT key and seeded admin password before any non-local deploy.
- Keep Swagger enabled only in development (`app.Environment.IsDevelopment()`).
- Keep permissive CORS (`AllowAnyOrigin/Method/Header`) unless a product-specific requirement says otherwise.
- A local PostgreSQL instance is provided via `docker-compose.yml`.

## Documentation

- Keep root `README.md` up to date when adding modules, config keys, or reusable services.
- Keep `AGENTS.md` updated when project-wide conventions change.
