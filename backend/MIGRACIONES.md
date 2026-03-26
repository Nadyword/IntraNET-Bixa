# 📦 Guía de Migraciones - Entity Framework Core 10

Esta guía explica cómo trabajar con migraciones en **Entity Framework Core 10** para la solución **IntranetCorp**.

---

## 🚀 Requisitos Previos

1. **.NET 10 SDK** instalado
2. **SQL Server LocalDB** o SQL Server Express disponible
3. Proyecto **IntranetCorp.API** como startup project
4. **Microsoft.EntityFrameworkCore.Design** instalado en el proyecto API (se instala automáticamente)

---

## 📍 Estructura de Proyectos

```
IntranetCorp.sln
├── IntranetCorp.Domain/           ← Entidades (sin migraciones)
├── IntranetCorp.Application/      ← DTOs y Handlers (sin migraciones)
├── IntranetCorp.Infrastructure/   ← DbContext + Migraciones ✓
└── IntranetCorp.API/              ← Controllers (Startup Project)
```

**Importante**: El DbContext (`AppDbContext`) está en `IntranetCorp.Infrastructure`, por lo que las migraciones se generan allí.

---

## 📋 Paso 1: Configuración Inicial

### 1.1 Verificar que la cadena de conexión esté en `appsettings.json`

**Ruta**: `IntranetCorp.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=IntranetCorp;Trusted_Connection=True;"
  },
  "Logging": { ... }
}
```

**Opciones de servidor**:
- LocalDB: `Server=(localdb)\\mssqllocaldb`
- SQL Server Express: `Server=localhost\\SQLEXPRESS` o `Server=.\\SQLEXPRESS`
- SQL Server: `Server=localhost;User Id=sa;Password=YourPassword;`

### 1.2 Verificar que Program.cs registre el DbContext

**Ruta**: `IntranetCorp.API/Program.cs`

```csharp
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
```

---

## 🎯 OPERACIONES COMUNES DE MIGRACIONES

### ✅ CREAR UNA MIGRACIÓN

Después de cambios en las entidades, crea una migración con nombre descriptivo:

```bash
cd C:\Users\samue\Documents\Proyectos\Productos BIXA\IntranetCorp\backend

dotnet ef migrations add NombreDeLaMigracion --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

**Ejemplos**:
```bash
# Primera migración
dotnet ef migrations add MigracionInit --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API

# Agregar nueva tabla
dotnet ef migrations add AgregarTablaUsuarios --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API

# Modificar columna
dotnet ef migrations add AgregarCampoFechaEliminacion --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

**¿Qué ocurre?**
- Se crea un archivo en `IntranetCorp.Infrastructure/Migrations/` con timestamp
- El archivo contiene `Up()` (cambios aplicar) y `Down()` (revertir)
- Se actualiza `AppDbContextModelSnapshot.cs` (snapshot actual del modelo)

**Archivos generados**:
```
Migrations/
├── 20260325150903_MigracionInit.cs           ← Cambios (Up/Down)
├── 20260325150903_MigracionInit.Designer.cs  ← Metadata
└── AppDbContextModelSnapshot.cs               ← Snapshot actual
```

---

### 🔄 APLICAR MIGRACIONES A LA BASE DE DATOS

**Opción A: Comando manual** (aplicar todas las migraciones pendientes)

```bash
cd C:\Users\samue\Documents\Proyectos\Productos BIXA\IntranetCorp\backend

dotnet ef database update --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

**Opción B: Automático al iniciar** (recomendado en desarrollo)

El archivo `IntranetCorp.API/Program.cs` ya contiene código automático:

```csharp
// Al iniciar la aplicación, aplica todas las migraciones
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}
```

Con esto, simplemente ejecuta:
```bash
dotnet run
```

Y la BD se crea/actualiza automáticamente.

**¿Qué ocurre?**
- Se aplica el DDL (Data Definition Language) a la BD
- Se registran las migraciones en tabla `__EFMigrationsHistory`
- Se crean todas las tablas, índices, relaciones FK, etc.

---

### ⏮️ REVERTIR A UNA MIGRACIÓN ANTERIOR

```bash
dotnet ef database update NombreMigracionAnterior --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

**Ejemplos**:
```bash
# Volver a la migración anterior
dotnet ef database update 20260324120000_MigracionAnterior --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API

# Eliminar todos los cambios (volver a estado inicial)
dotnet ef database update 0 --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

---

### 🗑️ ELIMINAR UNA MIGRACIÓN NO APLICADA

Si creaste una migración pero **NO** la aplicaste aún:

```bash
dotnet ef migrations remove --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

**Efectos**:
- Borra el archivo de migración más reciente
- Actualiza `AppDbContextModelSnapshot.cs`
- **No afecta la BD** (porque la migración nunca se aplicó)

**Advertencia**: No puedes remover migraciones que ya fueron aplicadas a producción.

---

### 📜 VER TODAS LAS MIGRACIONES

Ver lista de migraciones aplicadas y pendientes:

```bash
dotnet ef migrations list --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

**Salida esperada**:
```
20260325150903_MigracionInit (Applied)
20260326100000_AgregarTablaUsers (Pending)
```

---

### 🔍 VER EL SQL GENERADO

Ver el SQL que se ejecutará sin aplicarlo:

```bash
dotnet ef migrations script --output C:\temp\script.sql --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

O ver el SQL desde una migración específica:

```bash
dotnet ef migrations script 20260324120000_MigracionAnterior --output C:\temp\script.sql --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

---

### 🗄️ VER ESTADO DE LA BASE DE DATOS

Ver si la BD está actualizada o hay migraciones pendientes:

```bash
dotnet ef database update --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API --no-build
```

Si dice "No pending migrations", la BD está al día.

---

## 🔧 FLUJO DE TRABAJO TÍPICO

### Cambiar una entidad

**1. Modificar entidad** (ej: Tramite.cs)

```csharp
public class Tramite : BaseEntity
{
    // ... otros campos

    // Campo nuevo
    public string? CodigoTramite { get; set; }
}
```

**2. Crear migración**

```bash
cd C:\Users\samue\Documents\Proyectos\Productos BIXA\IntranetCorp\backend

dotnet ef migrations add AgregarCodigoTramite --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API
```

**3. Revisar el archivo generado** (opcional)

Abre `Migrations/[timestamp]_AgregarCodigoTramite.cs` y verifica que los cambios sean correctos.

**4. Aplicar la migración**

```bash
# Opción A: Manual
dotnet ef database update --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API

# Opción B: Automático (solo ejecuta la app)
dotnet run
```

**5. Commit** (si usas Git)

```bash
git add .\backend\IntranetCorp.Infrastructure\Migrations\
git commit -m "Agregar campo CodigoTramite a Tramite"
```

---

## ⚠️ ERRORES COMUNES Y SOLUCIONES

### Error 1: "No store type was specified for decimal property"

**Problema**: Columnas `decimal` sin precisión especificada

**Solución**: En `AppDbContext.OnModelCreating()`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Especificar precisión para columnas decimal
    modelBuilder.Entity<Tramite>()
        .Property(t => t.Monto)
        .HasPrecision(18, 2);  // 18 dígitos totales, 2 decimales
}
```

Luego crea una nueva migración.

---

### Error 2: "UserId cannot target the primary key 'Id' because it is not compatible"

**Problema**: Tipos de datos incompatibles en relaciones FK

**Solución**: Verifica que los tipos de FK coincidan con el PK de la tabla referenciada:

```csharp
// ❌ MALO: FK es Guid, pero ApplicationUser.Id es string
public class Tramite
{
    public Guid UserId { get; set; }  // ❌
    public virtual ApplicationUser? Usuario { get; set; }
}

// ✅ BIEN: FK es string, como ApplicationUser.Id
public class Tramite
{
    public string UserId { get; set; }  // ✅
    public virtual ApplicationUser? Usuario { get; set; }
}
```

---

### Error 3: "The Entity Framework tools version '10.0.3' is older than runtime '10.0.5'"

**Solución**: Actualizar EF Core CLI globalmente

```bash
dotnet tool update --global dotnet-ef
```

---

### Error 4: "Unable to create a DbContext of type AppDbContext"

**Solución**: Verifica que:

1. `IntranetCorp.API` esté configurado como startup project
2. Cadena de conexión en `appsettings.json` sea válida
3. Servidor SQL esté corriendo (`(localdb)`, SQL Server Express, etc.)

Prueba conexión:

```bash
sqlcmd -S (localdb)\mssqllocaldb
> SELECT 1;
> GO
```

---

## 📊 MONITOREAR LA BASE DE DATOS

### Ver tablas creadas

```bash
# Con Visual Studio: SQL Server Object Explorer
# Con SQL Server Management Studio: Object Explorer
# Con CLI sqlcmd:

sqlcmd -S (localdb)\mssqllocaldb -d IntranetCorp
> SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;
> GO
```

### Ver migraciones aplicadas

```bash
sqlcmd -S (localdb)\mssqllocaldb -d IntranetCorp
> SELECT * FROM __EFMigrationsHistory;
> GO
```

---

## 🎓 COMANDOS RÁPIDOS (CHEAT SHEET)

| Acción | Comando |
|--------|---------|
| Crear migración | `dotnet ef migrations add Nombre --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API` |
| Aplicar migraciones | `dotnet ef database update --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API` |
| Eliminar última migración (no aplicada) | `dotnet ef migrations remove --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API` |
| Revertir a migración anterior | `dotnet ef database update MigracionAnterior --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API` |
| Ver todas las migraciones | `dotnet ef migrations list --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API` |
| Generar script SQL | `dotnet ef migrations script --output script.sql --project IntranetCorp.Infrastructure --startup-project IntranetCorp.API` |
| Ejecutar aplicación (aplica migraciones automáticamente) | `dotnet run` (desde carpeta IntranetCorp.API) |

---

## 📚 REFERENCIAS

- [EF Core Migrations - Microsoft Docs](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations)
- [EF Core CLI Reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- [AppDbContext](./IntranetCorp.Infrastructure/Data/AppDbContext.cs) - Implementación local

---

## 🆘 SOPORTE

Si encuentras problemas:

1. Revisa los logs de error completos
2. Verifica que la cadena de conexión sea correcta
3. Confirma que SQL Server está corriendo
4. Limpia y reconstruye la solución: `dotnet clean && dotnet build`
5. Intenta remover la última migración y crearla nuevamente

---

**Última actualización**: 25 Mar 2026
