# IntranetCorp — Intranet Corporativa Full-Stack

Plataforma integral de gestión de recursos humanos para empresas, con backend .NET 10 y frontend React 18.

## 📋 Estructura del Proyecto

```
IntranetCorp/
├── backend/
│   ├── IntranetCorp.Domain/          # Entidades y enums
│   ├── IntranetCorp.Application/     # Lógica de aplicación (vacío, listo para DTOs y handlers)
│   ├── IntranetCorp.Infrastructure/  # EF Core, Servicios (Email, PDF, ExternalAPI, etc.)
│   └── IntranetCorp.API/             # Controllers, Program.cs, appsettings.json
└── frontend/
    └── intranet-client/
        ├── src/
        │   ├── components/
        │   │   ├── layout/           # Sidebar, TopHeader, AppLayout
        │   │   └── ui/               # Toast
        │   ├── pages/                # LoginPage, HomePage, MyDataPage, StubPage
        │   ├── store/                # Zustand stores (auth, ui)
        │   ├── lib/                  # axios config con interceptors
        │   ├── styles/               # globals.css con tokens CSS
        │   ├── App.tsx               # Router + section switching
        │   └── main.tsx              # Entry point
        ├── .env                      # Configuración producción
        └── .env.local                # Configuración desarrollo
```

---

## 🚀 Backend (.NET 10)

### Características implementadas

✅ **Clean Architecture**: Domain → Application → Infrastructure → API
✅ **Entity Framework Core** con SQL Server LocalDB
✅ **ASP.NET Core Identity** para autenticación
✅ **JWT Tokens** (Access Token 15min + Refresh Token en Cookie HttpOnly)
✅ **Soft Delete Global** — IsDeleted en todas las entidades
✅ **Servicios integrados**:
  - EmailService (SMTP Gmail con MailKit)
  - PdfService (QuestPDF)
  - ExternalApiService (HttpClient + IMemoryCache)
  - FileStorageService (App_Data/Uploads)
  - ChatbotFaqService (15+ respuestas predefinidas)

✅ **AuthController** con endpoints de Login/Register/SetPassword
✅ **CORS configurado** para localhost:5173 (frontend)
✅ **Swagger/OpenAPI** documentación automática

### Entidades de Base de Datos

```csharp
- ApplicationUser         (extends IdentityUser)
- Tramite                 (Vacaciones, Finiquito, Constancia, etc.)
- Documento               (archivos adjuntos a trámites)
- Announcement            (anuncios corporativos)
- Notification            (notificaciones de usuario)
- OnboardingStep          (pasos de incorporación)
```

### Para ejecutar el backend

```bash
cd backend/IntranetCorp.API

# Crear base de datos y ejecutar migraciones
dotnet ef database update

# Ejecutar servidor
dotnet run
# Disponible en https://localhost:5001 y http://localhost:5000
```

**Swagger UI**: https://localhost:5001/swagger

---

## 💻 Frontend (React 18 + TypeScript)

### Características implementadas

✅ **Vite** build tool con HMR
✅ **React Router v6** para navegación
✅ **Zustand** para estado global (auth + UI)
✅ **axios** con interceptors (JWT attachment + manejo de 401/403)
✅ **React Hook Form** + Zod para validación
✅ **TanStack Query** preparado en lib
✅ **Fidelidad exacta al HTML**:
  - Paleta: --red #D91A1A, --black #0D0D0D, --gray-700 #3D3D3D
  - Tipografía: Bebas Neue (headers) + DM Sans (body) + DM Mono (monospace)
  - Animaciones: transiciones de 0.2s
  - Responsive: sidebar colapsable en < 900px

### Componentes creados

**Layout**:
- Sidebar (navegación dark, colapsable, active states)
- TopHeader (search bar, notificaciones, user avatar, logout)
- AppLayout (wrapper principal)

**UI**:
- Toast (notificaciones automáticas)
- Botones reutilizables

**Páginas**:
- LoginPage (split-screen: red brand + login form)
- HomePage (hero banner + quick access + resumenes)
- MyDataPage (datos personales y laborales)
- StubPages (Cultura, Consultas, Solicitudes, Trámites, Líder, Chatbot, Onboarding)

### Para ejecutar el frontend

```bash
cd frontend/intranet-client

# Instalar dependencias
npm install

# Servidor de desarrollo (Hot Module Reloading)
npm run dev
# Disponible en http://localhost:5173
```

---

## 🔐 Flujo de autenticación

```
1. Usuario ingresa email + contraseña en LoginPage
2. POST /api/auth/login → backend valida y retorna JWT + user info
3. Frontend guarda JWT en localStorage y en Zustand store
4. axios interceptor attach Bearer token en cada request
5. Si 401 → interceptor redirige a /login y limpia session
```

---

## 📊 Base de datos

Conexión por defecto:
```
Server=(localdb)\mssqllocaldb
Database=IntranetCorp
Trusted_Connection=True
```

**Variables de configuración** en `backend/IntranetCorp.API/appsettings.json`:
- `Jwt:Key` — Secret para firmar tokens (min 32 caracteres)
- `Email:User`, `Email:AppPassword` — Credenciales Gmail
- `ExternalApi:BaseUrl` — Placeholder para API externa de cálculos LOTTT

---

## 🎯 Próximos pasos para completar

### Backend
- [ ] Implementar DTOs y MediatR handlers para cada Feature (Tramites, Users, Announcements, etc.)
- [ ] Migrations para cada entidad
- [ ] Email templates para notificaciones
- [ ] Integración con API externa real de cálculos
- [ ] Tests unitarios y de integración

### Frontend
- [ ] Implementar React Query hooks para data fetching
- [ ] Completar las páginas stub con componentes reales
- [ ] ChatWidget funcional
- [ ] Modal de solicitudes con validación
- [ ] Status tracker interactivo
- [ ] Tests con Vitest/React Testing Library

---

## 🛠 Stack resumido

| Layer | Tecnología |
|-------|-----------|
| API | .NET 10, ASP.NET Core |
| Auth | JWT + HttpOnly Cookies |
| BD | SQL Server / LocalDB |
| ORM | EF Core 10 |
| Frontend | React 18 + TypeScript |
| Build | Vite 5 |
| HTTP Client | axios |
| State | Zustand |
| Forms | react-hook-form + Zod |
| Routing | React Router v6 |
| Server State | TanStack Query (setup listo) |
| Styling | CSS variables + Responsive design |

---

## 📝 Credenciales de prueba

**Login**:
- Email: `ana.garcia@empresa.com`
- Password: `••••••••` (cualquier contraseña funciona en mock)
- Roles: Colaborador / Líder / RRHH

---

## 📞 Notas importantes

1. **CORS**: Frontend y backend corren en origins diferentes — CORS está habilitado
2. **HTTPS**: Backend requiere certificado HTTPS en dev (dotnet dev-certs https --trust)
3. **External API**: El servicio de cálculos está mockeado — reemplazar con API real en `ExternalApiService.cs`
4. **Email**: Para que funcione, necesitas credenciales de Gmail con App Passwords
5. **Nombres**: Se usan "BIXA" en lugar de "MANUFACT" en toda la UI

---

## 📦 Construcción para producción

### Backend
```bash
cd backend/IntranetCorp.API
dotnet publish -c Release -o ./publish
```

### Frontend
```bash
cd frontend/intranet-client
npm run build
# Archivos estáticos en dist/
```

---

**Última actualización**: Marzo 2025
**Status**: MVP funcional — prototipo full-stack listo para desarrollo
