# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**IntraNET Bixa** is a corporate intranet application for employee communication and HR management. It consists of a React 19 + TypeScript frontend and a .NET 9 Web API backend, using SQL Server as the database.

## Commands

### Frontend (`/frontend`)

```bash
npm run dev        # Dev server at http://localhost:5173
npm run build      # tsc -b && vite build → /dist
npm run lint       # ESLint with TypeScript rules
npm run preview    # Preview production build locally
```

### Backend (`/backend`)

```bash
# From solution root or specifying project path
dotnet build
dotnet run --project backend/Bixa.Backend/Bixa.Backend.csproj

# EF Core migrations
dotnet ef migrations add <NombreMigracion> --project backend/Bixa.Backend.DataAccess --startup-project backend/Bixa.Backend
dotnet ef database update --project backend/Bixa.Backend.DataAccess --startup-project backend/Bixa.Backend
```

Swagger UI is available at `https://localhost:5001/` when running in Development.

## Architecture

### Solution Structure

```
IntraNET Bixa/
├── frontend/              # React 19 + TypeScript + Vite
└── backend/
    ├── BixaIntraNet.slnx
    ├── Bixa.Backend/          # API layer (controllers, middleware, DI config)
    ├── Bixa.Backend.Services/ # Business logic
    ├── Bixa.Backend.DataAccess/ # EF Core, repositories, migrations
    └── Bixa.Backend.Models/   # DTOs, domain models, FluentValidation validators
```

### Backend Layer Flow

`Controllers` → `Services` (business logic) → `Repositories` (data access via EF Core) → SQL Server

- Dependency injection wired in `Bixa.Backend/Configuration/ServiceRegistrationExtensions.cs`
- `IUnitOfWork` manages transaction scoping
- AutoMapper profiles in `Bixa.Backend.Services/Mapper/BllMappingProfile.cs`

### API Response Convention

All endpoints return `ApiResponse<T>` (defined in `Bixa.Backend/Models/ApiResponse.cs`) with fields: `Success`, `Message`, `Data`, `StatusCode`. Use the static factory methods: `SuccessResponse()`, `BadRequest()`, `NotFoundResponse()`, `ErrorResponse()`, `UnauthorizedResponse()`, `Forbidden()`, `ConflictResponse()`.

### Auth endpoints (backend)

| Endpoint | Body | Descripción |
|---|---|---|
| `POST /api/login/AuthenticateLogin` | `{taxId, password}` | Login; si es primer acceso devuelve `{token: guid, refreshToken: "FIRSTLOGIN"}` |
| `POST /api/login/FirstLogin` | `{taxId, newPassword, token}` | Establece contraseña inicial; devuelve JWT real |
| `POST /api/login/RefreshToken` | `{refreshToken}` | Rota el token |
| `POST /api/login/validateToken` | `{token}` | Valida firma/expiración del JWT |

### Authentication

- JWT (HS256) with 60-minute expiration and refresh token rotation
- Key endpoints: `POST /api/login/Authenticate`, `/api/login/FirstLogin`, `/api/login/RefreshToken`, `/api/login/validateToken`
- JWT config (secret key, issuer, audience) lives in `appsettings.Development.json` under `ConfiguracionJwt`
- Frontend stores `accessToken` in localStorage via Zustand (`frontend/src/store/authStore.ts`)
- Axios interceptor in `frontend/src/lib/api.ts` auto-logs out on 401/403
- Authorization uses both role (`hasRole()`) and permission (`hasPermission()`) checks from JWT claims

### Frontend State Management

- **Zustand**: `authStore.ts` (auth state + user info decoded from JWT), `uiStore.ts` (sidebar, notifications, toasts)
- **TanStack React Query**: server state and caching for API calls
- **React Hook Form + Zod**: form validation throughout the app
- **Axios** instance (`/lib/api.ts`): base URL from `VITE_API_URL` env var, request/response interceptors

### Frontend Routing

Defined in `frontend/src/App.tsx`. Protected routes check `isAuthenticated` from `authStore`. Main pages: Home, MyData, Culture, Consultas, Solicitudes, Trámites, Leader, Chat, Onboarding, Admin (roles).

### FluentValidation

Validators live in `Bixa.Backend.Models/DTOs/Validators/` and auto-validate on controller entry via `AddFluentValidationAutoValidation()` middleware — no manual `ModelState` checking needed in controllers.

## Configuration

| File | Purpose |
|------|---------|
| `backend/Bixa.Backend/appsettings.Development.json` | DB connection string, JWT secret, CORS origins, Serilog config |
| `frontend/.env` | `VITE_API_URL=https://localhost:5001/api` |

CORS allowed origins and IPs are configured in `appsettings` under `AllowedOrigins` / `AllowedIPs`, consumed by `CorsConfigurator.cs`.
