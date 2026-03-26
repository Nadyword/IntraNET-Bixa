namespace IntranetCorp.Domain.Constants;

/// <summary>
/// Catálogo de permisos del sistema. Estos se almacenan como claims en AspNetRoleClaims
/// con ClaimType="permission" y ClaimValue=permiso.Name.
/// </summary>
public static class SystemPermissions
{
    public static readonly Permission[] All =
    [
        new Permission("tramites.leer", "Ver trámites", "Trámites"),
        new Permission("tramites.crear", "Crear trámites", "Trámites"),
        new Permission("tramites.actualizar", "Editar trámites", "Trámites"),
        new Permission("tramites.borrar", "Eliminar trámites", "Trámites"),

        new Permission("usuarios.ver", "Ver directorio de usuarios", "Usuarios"),
        new Permission("usuarios.crear", "Crear empleados", "Usuarios"),
        new Permission("usuarios.editar", "Editar usuarios", "Usuarios"),
        new Permission("usuarios.borrar", "Eliminar usuarios", "Usuarios"),

        new Permission("nominas.ver", "Ver información de nóminas", "Nóminas"),

        new Permission("anuncios.crear", "Publicar anuncios", "Anuncios"),
        new Permission("anuncios.borrar", "Eliminar anuncios", "Anuncios"),

        new Permission("roles.gestionar", "Gestionar permisos de roles", "Administración"),
    ];

    public record Permission(string Name, string Description, string Category);
}
