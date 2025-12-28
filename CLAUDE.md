# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Aloe Medock is a medical reservation management system built with Blazor Server, targeting closed network environments (閉域環境). The system manages healthcare facility appointments with calendar visualization, real-time updates via SignalR, and multi-tenant support.

**Technology Stack:**
- .NET 10 / C# 14
- Blazor Server (interactive server-side rendering)
- PostgreSQL 18+ with EF Core
- Cookie authentication
- Tailwind CSS + daisyUI (via CDN, no Node.js)
- Canvas API (native Canvas 2D rendering for calendar visualization)

## Architecture

### Project Structure

```
src/Aloe/
├── Apps/Medock/
│   ├── Aloe.Apps.MedockServer/     # Blazor Server + REST API
│   │   ├── Components/             # Razor components
│   │   │   ├── Pages/             # Page components
│   │   │   ├── Layout/            # Layout components
│   │   │   └── Calendar/          # Calendar components
│   │   ├── Controllers/           # REST API endpoints
│   │   └── Program.cs             # App configuration & DI
│   ├── Aloe.Apps.MedockLib/       # Core business logic library
│   │   ├── Data/                  # EF Core DbContext & entities
│   │   ├── Services/              # Business services
│   │   └── Repositories/          # Data access layer
│   └── Aloe.Apps.MedockSeed/      # Database seeding tool
└── Tests/
    └── Aloe.Apps.MedockServer.Tests/  # xUnit tests
```

### Architectural Patterns

**MVVM + Onion Architecture:**
- **View:** `.razor` files use loose `@bind` for data binding
- **ViewModel:** `.razor.cs` code-behind holds display-related state
- **Model:** Separate service/repository layers for business logic

**Database Layer:**
- EF Core with PostgreSQL provider (Npgsql)
- Entities follow snake_case naming for DB columns (e.g., `user_id`, `created_at`)
- Automatic audit tracking via `IAuditableEntity` interface
- Soft deletes via `is_deleted` flag
- Primary keys use `Guid.CreateVersion7()` for time-ordered UUIDs
- JSONB support for complex data (e.g., `appt_graph` column)

**DbContext Lifecycle (Blazor Server):**
- `IDbContextFactory<MedockDbContext>` for components (short-lived contexts)
- Scoped `MedockDbContext` for repositories
- Audit fields auto-updated in `SaveChangesAsync()` via `IUserContextService`

**Authentication:**
- Cookie authentication with ASP.NET Core
- Support for Issue, Refresh, Revoke operations
- Session tracking with `sessions` table
- PBKDF2-SHA256 password hashing (10,000 iterations)

## Development Commands

### Database Setup

**Initial Setup (first time only):**
```cmd
CD sql\pg_setup\
.\00_setup.bat
```

This runs:
1. Extension setup (`ext_create_extensions.sql`)
2. Table creation (`01_03_create_appt_tables_sjis.sql`)
3. Index creation (`02_01_create_indexes_sjis.sql`)
4. Trigger creation (`03_01_create_log_triggers_sjis.sql`, etc.)

**PostgreSQL Configuration:**
- Install PostgreSQL 18+
- Add bin to PATH: `C:\Program Files\PostgreSQL\17\bin`
- Copy `sql\pg_setup\pgpass.conf` to `%APPDATA%\postgresql\`
- Consider using PGTune for optimization

### Building & Running

**ソリューションファイル (.NET 10 slnx形式):**
```
src/Aloe/Aloe.slnx  ← これを使う
```

**Run the seed data generator:**
```bash
dotnet run --project src/Aloe/Apps/Medock/Aloe.Apps.MedockSeed
```

**Run the server:**
```bash
dotnet run --project src/Aloe/Apps/Medock/Aloe.Apps.MedockServer
```

**Build all projects:**
```bash
dotnet build src/Aloe/Aloe.slnx
```

### Testing

**Run all tests:**
```bash
dotnet test src/Aloe/Tests/Aloe.Apps.MedockServer.Tests
```

**Run specific test:**
```bash
dotnet test --filter "FullyQualifiedName~Aloe.Apps.MedockServer.Tests.Services.AuthServiceTests.LoginAsync_ValidCredentials_ReturnsSuccess"
```

**Run with coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Code Conventions

**Follow C# Coding Guidelines 2025:**
https://qiita.com/Ted-HM/items/1d4ecdc2a252fe745871

**Key Conventions:**
- Use snake_case for database column names in EF configuration
- C# entities use PascalCase properties
- Japanese comments are acceptable in domain logic
- Explicit column name mapping via `.HasColumnName()`

**Entity Example:**
```csharp
entity.Property(e => e.UserId).HasColumnName("user_id");
entity.Property(e => e.UserCode).HasColumnName("user_code").HasMaxLength(100);
```

## Important Implementation Notes

### Authentication Flow

1. Login via `AuthService.LoginAsync()` creates a session
2. Session created in `sessions` table with `session_id`
3. Cookie authentication with claims: `sub` (user_id), `user_code`, `email`, `tenant_id`, `facility_id`, `roles`, `session_id`
4. Session refresh via `RefreshTokenAsync()` updates cookie expiration
5. Account lockout after 5 failed attempts (15 minutes)
6. `CustomAuthenticationStateProvider` revalidates every 5 minutes

### Audit Tracking

All entities implementing `IAuditableEntity` automatically track:
- `created_at`, `created_user_id`, `created_session_id`
- `updated_at`, `updated_user_id`, `updated_session_id`

Set audit context via `MedockDbContext.SetAuditInfo(userId, sessionId)` before `SaveChanges()`.

### Multi-Tenant Design

- Users belong to tenants via `tenant_users` table
- `FacilityUser` links users to facilities; `FacilityUserRole` assigns roles
- System admins (`is_system_admin = true`) can access multiple tenants/facilities
- Regular users typically have one tenant
- Tenant selection screen shown only for multi-tenant users
- `UserContextService.InitializeFromClaimsAsync()` handles facility fallback

### CSS Framework (No Node.js)

**Development (CDN):**
```html
<script src="https://cdn.tailwindcss.com"></script>
<link href="https://cdn.jsdelivr.net/npm/daisyui@5/dist/full.min.css" rel="stylesheet" />
```

**Production:** Use LibMan or manually download built CSS.

**Responsive Design:**
Use `@container` queries (not `@media`) for component-based responsive behavior:
```css
.responsive-container { container-type: inline-size; }

@container (min-width: 768px) {
  .sidebar { display: block; }
}
```

### Calendar Implementation

**Architecture:**
- Canvas 2D API for high-performance calendar rendering (replaced Konva.js)
- Layered canvas system:
  - `background`: Static backgrounds and cell styling
  - `grid`: Grid lines and axis labels
  - `content`: Data visualization (bars, charts, text)
  - `interaction`: Hit detection and hover effects
- ES6 modules: `canvas-month-view.js`, `canvas-year-view.js`, `canvas-week-view.js`, `canvas-day-detail.js`
- RenderState pattern: Maintains hit-test data (cells, bars, slots) for click detection
- CanvasManager: Handles multi-layer canvas creation, resizing, and coordinate transformation

**Features:**
- Year/Month/Week views with real-time slot visualization
- Day detail popup (separate Canvas instance) for appointment bar chart display
- SignalR Hub for real-time collaboration (future: show other users' cursor positions)
- Slot bar charts with:
  - Winter color map for vacancy visualization
  - Capacity lines and availability indicators
  - Time slot grouping with lunch break support
  - Responsive symbol-based simple view mode (×, △, ○, ◎)
- Week scheduler supports 1/3/7/14/31 day ranges

**File Structure:**
```
wwwroot/js/calendar/
├── calendar-main.js          # Public API & initialization
├── config.js                 # Configuration & color mappings
├── state.js                  # Global application state
├── renderers/
│   ├── canvas-month-view.js      # Month grid + day charts
│   ├── canvas-year-view.js       # 12-month grid layout
│   ├── canvas-week-view.js       # Week scheduler view
│   ├── canvas-day-detail.js      # Day detail popup graph
│   ├── canvas-bar-chart.js       # Reusable bar chart component
│   ├── canvas-month-calendar.js  # Month grid helper
│   ├── canvas-interactions.js    # Click/hover handlers
│   └── canvas-render-state.js    # Hit-test state management
├── ui/
│   ├── canvas-manager.js     # Multi-layer canvas management
│   ├── layers.js             # Layer setup
│   └── tooltip.js            # Hover tooltips
├── utils/
│   ├── canvas-utils.js       # Drawing primitives (rect, line, text, etc.)
│   ├── date-utils.js         # Date calculations
│   ├── winter-colormap.js    # Vacancy color mapping
│   └── math-utils.js         # Geometric calculations
└── realtime/
    └── signalr-client.js     # Real-time data updates
```

**Important Notes:**
- Do NOT use global `renderState.reset()` during popup rendering (it affects main calendar hit detection)
- Slot data properties: `cap` (capacity), `count` (used), `start` (HH:mm), `end` (HH:mm), `isOutsideHours`, `isGrayedOut`
- Popup dialogs use separate Canvas Manager instances and do not share render state with main calendar
- Color system: Uses config.js for consistent styling across all views

### Google Fonts for Closed Networks

For closed network deployments, self-host M PLUS 1 Code font:
1. Download from [google-webfonts-helper](https://gwfh.mranftl.com/fonts/m-plus-1-code)
2. Place in `wwwroot/fonts/`
3. Replace CDN link in `App.razor` with local `@font-face`

## Common Pitfalls

1. **EF Column Names:** Always specify `.HasColumnName()` - database uses snake_case
2. **Audit Info:** Call `SetAuditInfo()` before any SaveChanges if you need audit tracking
3. **Soft Deletes:** Use `is_deleted` flag, never hard delete. Use filtered unique indexes: `.IsUnique().HasFilter("[is_deleted] = 0")`
4. **Tenant Context:** Always filter by `tenant_id` for multi-tenant queries
5. **Cookie Configuration:** Ensure `CookieSettings` section exists in `appsettings.json`
6. **Connection Strings:** Use User Secrets for sensitive config in development
7. **Read Operations:** Use `.AsNoTracking()` for queries; use `FindForUpdateAsync()` when modifications needed

## User Secrets

For local development with sensitive data:
```bash
dotnet user-secrets init --project src/Aloe/Apps/Medock/Aloe.Apps.MedockServer
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=medock;..." --project src/Aloe/Apps/Medock/Aloe.Apps.MedockServer
```

## Test-Driven Development Approach

This project follows a test-first philosophy:

**For AI Implementation:**
1. Define test structure with failing tests
2. Implement features to make tests pass incrementally

**For Manual Implementation:**
1. Write failing test (Red)
2. Implement minimal code to pass (Green)
3. Refactor while keeping tests green (Refactor)

Test framework: xUnit with Moq, FluentAssertions, and EF InMemory provider.

## Debugging

**SQL Logging:** Control via `Features:ShowSqlLogs` in appsettings.json or environment variable `Features__ShowSqlLogs`. Default is false.
