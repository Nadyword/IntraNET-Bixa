# Bixa Backend

## Descripción
Bixa Backend es una solución basada en .NET 9 diseñada para gestionar y procesar solicitudes de manera eficiente. La arquitectura está organizada en capas, lo que facilita la escalabilidad, el mantenimiento y la separación de responsabilidades.

## Tabla de Contenido
- [Descripción](#descripción)
- [Tabla de Contenido](#tabla-de-contenido)
- [Instalación](#instalación)
- [Despliegue](#despliegue)
- [Uso](#uso)
- [Características](#características)
- [Configuración](#configuración)
- [Licencia](#licencia)

## Instalación
1. **Clona el repositorio:**
   ```bash
   git clone https://github.com/SoftwareenFacil/ryq-backend.git
   ```
2. **Navega al directorio del proyecto:**
   ```bash
   cd ryq-backend
   ```
3. **Restaura las dependencias:**
   ```bash
   dotnet restore
   ```

4. **Configura la base de datos y aplica las migraciones:**

   - **Aplica las migraciones existentes:**
     ```bash
     dotnet ef database update -p Bixa.Backend.DataAccess -s Bixa.Backend
     ```

   - **Comandos de Entity Framework:**

     - Para crear una nueva migración:
       ```bash
       dotnet ef migrations add MigrationV29P --context AppDbContext -p Bixa.Backend.DataAccess -s Bixa.Backend
       ```

     - Para aplicar las migraciones y actualizar la base de datos:
       ```bash
       dotnet ef database update --context AppDbContext -p Bixa.Backend.DataAccess -s Bixa.Backend
       ```
   ```
 
 ## Despliegue
  ```bash
    dotnet publish backend/Bixa.Backend/Bixa.Backend.csproj -c Release -r linux-x64 --self-contained false -o ./Publicacion
  ```

## Uso
1. **Ejecuta el proyecto:**
   ```bash
   dotnet run --project Bixa.Backend
   ```
2. **Accede a la API en tu navegador o cliente HTTP en:**
   ```                                                                                                                                                                                                      i
   https://localhost:7194;http://localhost:5108
   ```
3. **Explora la documentación Swagger disponible en:**
   ```
   http://localhost:7194/index.html
   ```

## Características
- Arquitectura por capas con separación de responsabilidades.
- Integración con Entity Framework Core para acceso a datos.
- Autenticación y autorización mediante JWT.
- Documentación automática con Swagger.
- Configuración flexible para entornos de desarrollo y producción.

## Configuración
1. **Configura las variables de entorno necesarias en `appsettings.json` o mediante variables de entorno del sistema:**
   - `WebApiDatabase`: Cadena de conexión a la base de datos.
   - `APP_VERSION`: Versión de la aplicación.
2. **Personaliza los valores según el entorno (desarrollo, producción, etc.).**

## Licencia
Este proyecto está licenciado bajo la [Licencia MIT](LICENSE).Este proyecto está licenciado bajo la [Licencia MIT](LICENSE).