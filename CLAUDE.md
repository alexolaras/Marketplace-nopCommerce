# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

nopCommerce — an open-source ASP.NET Core eCommerce platform. Targets **.NET 10** (`net10.0`, SDK `10.0.100` pinned in `global.json`). Supports MS SQL Server, PostgreSQL, and MySQL.

## Build & run

All `dotnet` commands below assume you're in `src/` (where `NopCommerce.sln` lives).

```bash
# Restore + build the whole solution
dotnet build NopCommerce.sln -c Release

# Run the store (public + admin). App is the Nop.Web project.
dotnet run --project Presentation/Nop.Web/Nop.Web.csproj

# Publish (what the Dockerfile does)
dotnet publish Presentation/Nop.Web/Nop.Web.csproj -c Release -o ./published

# Docker: multi-stage build producing a runnable container
docker build -t nopcommerce .           # from repo root
docker compose up                        # compose file in repo root (also mysql-/postgresql-docker-compose.yml)
```

On first run the app redirects to `/install` to create `App_Data/dataSettings.json` (connection string + provider). Without it, most commands against a running instance won't work.

## Tests

A single test project using **NUnit 4 + Moq + AwesomeAssertions**, with **SQLite** as the in-memory backing store (see `Tests/Nop.Tests/SqLiteNopDataProvider.cs` and `BaseNopTest.cs`).

```bash
# Run all tests
dotnet test Tests/Nop.Tests/Nop.Tests.csproj

# Run a single test / filter
dotnet test Tests/Nop.Tests/Nop.Tests.csproj --filter "FullyQualifiedName~CustomerServiceTests"
dotnet test Tests/Nop.Tests/Nop.Tests.csproj --filter "Name=Can_save_customer"
```

Tests are organised under `Tests/Nop.Tests/Nop.Core.Tests`, `Nop.Data.Tests`, `Nop.Services.Tests`, `Nop.Web.Tests`. `BaseNopTest` bootstraps a full `NopEngine` against SQLite — most service tests inherit from it and resolve services via DI rather than newing them up.

## Architecture

Layered, plugin-oriented monolith. Dependencies flow `Nop.Web` → `Nop.Web.Framework` → `Nop.Services` → `Nop.Data` → `Nop.Core`.

### Layers (`src/Libraries` + `src/Presentation`)

- **`Nop.Core`** — domain entities (`Domain/`), `BaseEntity`, caching abstractions, events, configuration (`AppSettings`), and the **DI engine** in `Infrastructure/`. `NopEngine` + `EngineContext` is the composition root; `ITypeFinder` scans assemblies (including plugin DLLs) for `INopStartup` / `IStartupTask` implementations to auto-register services. `Singleton<T>` is the global service-locator escape hatch used at startup.
- **`Nop.Data`** — persistence. Uses **LinqToDB** (not EF Core) as the ORM; migrations are driven by **FluentMigrator**. One data provider per DB (`MsSqlDataProvider`, `PostgreSqlDataProvider`, `MySqlDataProvider` in `DataProviders/`). `DataProviderManager` picks the implementation from `App_Data/dataSettings.json`. Repositories are generic (`EntityRepository<T> : IRepository<T>`) — there is no per-entity repository class.
- **`Nop.Services`** — business logic organised by domain area (`Catalog`, `Orders`, `Customers`, `Shipping`, `Tax`, `Payments`, `Messages`, `Media`, `Plugins`, `ScheduleTasks`, `Localization`, `Caching`, …). Service interfaces + implementations live side-by-side; most are registered automatically by convention via `NopStartup` in `Nop.Web.Framework`.
- **`Nop.Web.Framework`** — MVC/Razor glue: model binders, action filters, themeing, validation (FluentValidation), AutoMapper profiles, the `INopStartup` pipeline configuration, and admin-area scaffolding.
- **`Nop.Web`** (the executable) — two presentations in one process:
  - Public storefront: `Controllers/`, `Views/`, `Models/`, `Factories/` (model factories are a strong nop convention — keep controllers thin and push model assembly into `*ModelFactory`).
  - Admin: under `Areas/Admin/` with its own Controllers/Views/Models/Factories.
  - `Program.cs` is minimal; actual pipeline/services wiring happens through `ConfigureApplicationSettings` / `ConfigureApplicationServices` / `ConfigureRequestPipeline` which iterate every `INopStartup` found in referenced + plugin assemblies (ordered). If you add startup logic, add an `INopStartup` — do not edit `Program.cs`.
  - Autofac is optionally used as the DI container (toggled by `CommonConfig.UseAutofac` in `App_Data/appsettings.json`).

### Plugins (`src/Plugins/*`)

Each plugin is its own csproj that compiles into `Presentation/Nop.Web/Plugins/<Name>/` (see the `NopTarget` MSBuild target + `Build/ClearPluginAssemblies.proj` — this is how duplicate framework DLLs are stripped from plugin output). A plugin contains a `plugin.json` manifest, usually a `*Plugin.cs` entry class implementing a domain-specific interface (`IPaymentMethod`, `IShippingRateComputationMethod`, `IWidgetPlugin`, `IExternalAuthenticationMethod`, `ITaxProvider`, `IMiscPlugin`, …), plus its own Controllers/Views/Models/Services.

When adding or touching a plugin: keep its code self-contained inside its project directory — don't bleed plugin-specific types into `Nop.Services` or `Nop.Web`. The runtime discovers the plugin's startup via the same `INopStartup` scan described above.

### Data flow conventions

- Controllers take constructor-injected services and model factories; they do not query `IRepository<T>` directly.
- Services expose **async** methods end-to-end (`*Async` naming is the norm).
- Entities inherit `BaseEntity` (int `Id`); LinqToDB mappings live in `Nop.Data/Mapping/`.
- Caching goes through `IStaticCacheManager` with keys defined in per-area `*CacheDefaults` / `*CacheEventConsumer` classes; cache invalidation is event-driven (`Nop.Core.Events` + `IConsumer<T>`).
- Cross-cutting concerns (logging, settings, localization, events) are resolved via DI — avoid static access except for `Singleton<T>` during very-early startup.

## Notes that matter

- `NopCommerce.sln` contains every library, the web host, tests, and **all plugins** — solution builds are slow; for iteration, build a specific csproj.
- The admin area is **not** a separate app, it's an `Area` inside `Nop.Web`.
- Frontend assets under `Nop.Web/wwwroot` are bundled via a gulp pipeline (`Nop.Web/gulpfile.js`, `package.json`) — only relevant if you're changing CSS/JS bundles.
- `global.json` pins the SDK; if `dotnet build` fails with an SDK-not-found error, install .NET 10 SDK `10.0.100` or newer feature band.
