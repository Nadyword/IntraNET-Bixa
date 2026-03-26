# IntranetCorp Backend - .NET 10

Backend completo de la **Intranet Corporativa BIXA** construido con **.NET 10**, **Entity Framework Core 10** y **ASP.NET Core**.

---

## 📋 Contenido

- [Estructura del Proyecto](#estructura-del-proyecto)
- [Requisitos](#requisitos)
- [Instalación y Ejecución](#instalación-y-ejecución)
- [Migraciones](#migraciones)
- [Endpoints](#endpoints)
- [Entidades](#entidades)
- [Servicios](#servicios)
- [Configuración](#configuración)

---

## 🏗️ Estructura del Proyecto

```
IntranetCorp/
├── backend/
│   ├── IntranetCorp.sln                    ← Solución principal
│   ├── IntranetCorp.Domain/                ← Entidades y Enums
│   │   ├── Entities/
│   │   │   ├── ApplicationUser.cs          (extends IdentityUser)
│   │   │   ├── Tramite.cs
│   │   │   ├── Documento.cs
│   │   │   ├── Announcement.cs
│   │   │   ├── Notification.cs
│   │   │   └── OnboardingStep.cs
│   │   ├── Enums/
│   │   │   └── TramiteEnums.cs
│   │   └── Common/
│   │       └── BaseEntity.cs               (base con soft delete)
│   ├── IntranetCorp.Application/           ← DTOs y Handlers (vacío, listo para Features)
│   ├── IntranetCorp.Infrastructure/        ← EF Core, Servicios
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   └── Migrations/                 ← Migraciones de BD
│   │   └── Services/
│   │       ├── EmailService.cs
│   │       ├── PdfService.cs
│   │       ├── ExternalApiService.cs
│   │       ├── FileStorageService.cs
│   │       └── ChatbotFaqService.cs
│   └── IntranetCorp.API/                   ← Controladores y Configuración
│       ├── Controllers/
│       │   └── AuthController.cs
│       ├── Program.cs                      (configuración completa)
│       ├── appsettings.json
│       └── appsettings.Development.json
└── frontend/
    └── intranet-client/                    ← Ver README del frontend
```

---

## 📦 Requisitos

- **.NET 10 SDK** (o superior)
- **SQL Server LocalDB**, SQL Server Express, o SQL Server
- **Visual Studio 2022** o **Visual Studio Code**
- **Git** (opcional, para control de versiones)

### Verificar instalación

```bash
dotnet --version       # Debe mostrar v10.x.x
dotnet --list-runtimes # Debe incluir .NET 10
```

---

## 🚀 Instalación y Ejecución

### 1️⃣ Clonar/Abrir el Proyecto

```bash
cd C:\Users\samue\Documents\Proyectos\Productos\ BIXA\IntranetCorp\backend
```

### 2️⃣ Restaurar Dependencias

```bash
dotnet restore
```

### 3️⃣ Crear Base de Datos

Las migraciones se aplican automáticamente al iniciar. Ver sección [Migraciones](#migraciones).

### 4️⃣ Ejecutar el Servidor

```bash
# Desde carpeta IntranetCorp.API
cd IntranetCorp.API
dotnet run

# O desde la raíz del backend
dotnet run --project IntranetCorp.API
```

**Salida esperada**:

```
info: Microsoft.Hosting.Lifetime
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

### 5️⃣ Acceder a Swagger

Abre en navegador:
```
https://localhost:5001/swagger
```

---

## 🗄️ Migraciones

### ¿Qué son las migraciones?

Las migraciones son cambios versionados de la estructura de la base de datos. EF Core genera archivos con el SQL necesario para transformar la BD de un estado a otro.

### Comandos Principales

**Ver la guía completa**: [`MIGRACIONES.md`](./MIGRACIONES.md)

#### Crear una migración
```bash
cd C:\Users\samue\Documents\Proyectos\Productos\ BIXA\IntranetCorp\backend

dotnet ef migrations add NombreMigracion --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

#### Aplicar migraciones
```bash
# Opción A: Manual
dotnet ef database update --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API

# Opción B: Automático (al ejecutar dotnet run)
# Ya está configurado en Program.cs
```

#### Revertir a una migración anterior
```bash
dotnet ef database update NombreMigracionAnterior --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

#### Eliminar última migración (sin aplicar)
```bash
dotnet ef migrations remove --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

**Nota**: Para guía detallada con ejemplos, ver [`MIGRACIONES.md`](./MIGRACIONES.md)

---

## 📡 Endpoints

### Autenticación

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/login` | Login usuario |
| POST | `/api/auth/register` | Registrar nuevo usuario |
| POST | `/api/auth/set-password` | Establecer contraseña con token |
| POST | `/api/auth/refresh` | Refrescar JWT token |

**Body de ejemplo - Login**:
```json
{
  "email": "ana.garcia@empresa.com",
  "password": "Password123!"
}
```

**Response**:
```json
{
  "token": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "user": {
    "id": "user-id",
    "email": "ana.garcia@empresa.com",
    "nombre": "Ana García",
    "cargo": "Coordinadora",
    "departamento": "RRHH"
  }
}
```

---

## 📊 Entidades

### ApplicationUser
```csharp
public class ApplicationUser : IdentityUser
{
    public string Nombre { get; set; }
    public string Cedula { get; set; }
    public string Cargo { get; set; }
    public string Departamento { get; set; }
    public DateTime FechaIngreso { get; set; }
    public bool IsDeleted { get; set; }          // Soft Delete
    public DateTime? DeletedAt { get; set; }
    public virtual ICollection<Tramite> Tramites { get; set; }
}
```

### Tramite
```csharp
public class Tramite : BaseEntity
{
    public string UserId { get; set; }           // FK a ApplicationUser
    public TramiteTipo Tipo { get; set; }        // Enum
    public TramiteEstado Estado { get; set; }    // Enum
    public string Descripcion { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? Duracion { get; set; }
    public decimal? Monto { get; set; }
    public bool IsDeleted { get; set; }          // Soft Delete
}
```

### Documento
```csharp
public class Documento : BaseEntity
{
    public Guid TramiteId { get; set; }          // FK a Tramite
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }      // MIME type
    public long Size { get; set; }               // en bytes
}
```

### Notification, Announcement, OnboardingStep

Ver archivos individuales en `IntranetCorp.Domain/Entities/`

---

## 🔧 Servicios

Todos ubicados en `IntranetCorp.Infrastructure/Services/`

### EmailService
- Envía emails por SMTP Gmail
- Usa MailKit
- Plantillas HTML personalizables

**Uso**:
```csharp
await emailService.SendEmailAsync(
    toEmail: "user@empresa.com",
    subject: "Bienvenida a BIXA",
    body: "<h1>Bienvenido</h1>..."
);
```

### PdfService
- Genera PDFs con QuestPDF
- Ideal para trámites finalizados
- Customizable con logos y estilos

**Uso**:
```csharp
var pdfBytes = await pdfService.GenerateTramitePdfAsync(tramiteId);
```

### ExternalApiService
- Conecta con API externa de cálculos (ej: LOTTT)
- HttpClient con reintentos
- IMemoryCache para caché de 1 hora

**Uso**:
```csharp
var result = await externalApiService.GetVacacionDaysAsync(userId);
```

### FileStorageService
- Guarda archivos en `App_Data/Uploads/`
- Organiza por userId y tramiteId
- Manejo de eliminación segura

**Uso**:
```csharp
var path = await fileService.SaveFileAsync(fileName, fileBytes, userId, tramiteId);
```

### ChatbotFaqService
- 15+ preguntas frecuentes predefinidas
- Búsqueda por palabras clave
- Respuestas automáticas

**Uso**:
```csharp
var respuesta = chatbotService.GetFaqResponse("vacaciones");
```

---

## ⚙️ Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=IntranetCorp;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "REPLACE_WITH_SECURE_KEY_MIN_32_CHARS",
    "Issuer": "IntranetCorp",
    "ExpiryMinutes": 15
  },
  "RefreshToken": {
    "ExpiryDays": 30
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "User": "REPLACE_WITH_GMAIL",
    "AppPassword": "REPLACE_WITH_APP_PASSWORD"
  },
  "ExternalApi": {
    "BaseUrl": "https://PLACEHOLDER_API_URL/api",
    "CacheTtlMinutes": 60
  },
  "FileStorage": {
    "BasePath": "App_Data/Uploads"
  }
}
```

### Cambiar Base de Datos

**LocalDB** (por defecto):
```json
"Server=(localdb)\\mssqllocaldb;Database=IntranetCorp;Trusted_Connection=True;"
```

**SQL Server Express**:
```json
"Server=localhost\\SQLEXPRESS;Database=IntranetCorp;Trusted_Connection=True;"
```

**SQL Server con credenciales**:
```json
"Server=localhost;Database=IntranetCorp;User Id=sa;Password=YourPassword;"
```

---

## 🔐 Seguridad

✅ **Implementado**:
- JWT Authentication (15 min access token)
- Refresh Token en Cookie HttpOnly
- CORS restringido a orígenes autorizados
- Global soft delete (sin eliminar datos)
- Hash de contraseñas con Identity
- Roles (Colaborador, Líder, RRHH, Admin)

**Próximos pasos**:
- Validación con FluentValidation en todos los Commands
- Logging centralizado
- Rate limiting por IP
- HTTPS enforced en producción

---

## 🧪 Testing

Próximamente:
- Unit tests con NUnit
- Integration tests con test database
- Mock de servicios externos

---

## 📚 Documentación Adicional

- [`MIGRACIONES.md`](./MIGRACIONES.md) - Guía completa de migraciones
- [`IntranetCorp.Domain/`](./IntranetCorp.Domain/) - Definiciones de entidades
- [`IntranetCorp.Infrastructure/`](./IntranetCorp.Infrastructure/) - Servicios e implementación
- [`IntranetCorp.API/Program.cs`](./IntranetCorp.API/Program.cs) - Configuración completa

---

## 🆘 Solución de Problemas

### "Connection string error"
- Verifica que SQL Server esté corriendo
- Prueba conexión: `sqlcmd -S (localdb)\mssqllocaldb`

### "Unable to create DbContext"
- Asegúrate que IntranetCorp.API sea el startup project
- Verifica cadena de conexión en appsettings.json

### "Migraciones no se aplican"
- Ejecuta: `dotnet ef database update --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API`
- O simplemente ejecuta: `dotnet run` (se aplican automáticamente)

### "Email no se envía"
- Configura credenciales Gmail reales en appsettings.json
- USA App Password (no contraseña normal)
- Verifica que la cuenta tenga 2FA habilitado

---

## 📝 Changelog

**v1.0 (25 Mar 2026)**
- ✅ Estructura base de Clean Architecture
- ✅ Entidades con soft delete global
- ✅ Servicios de Email, PDF, API externa, File storage
- ✅ AuthController con JWT
- ✅ DbContext con migraciones
- ✅ Swagger/OpenAPI

---

## 🤝 Contribución

Para agregar nuevas funcionalidades:

1. Crea una entidad en `Domain/Entities/`
2. Agrega relaciones en `AppDbContext`
3. Crea una migración: `dotnet ef migrations add NombreFeature`
4. Implementa servicios en `Infrastructure/Services/`
5. Crea un Controller en `API/Controllers/`
6. Documenta en este README

---

## 📄 Licencia

Proyecto interno de BIXA - 2026

---

**Última actualización**: 25 Mar 2026
**Autor**: Samuel (Staff Software Engineer)
