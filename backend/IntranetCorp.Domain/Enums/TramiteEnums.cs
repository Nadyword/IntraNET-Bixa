namespace IntranetCorp.Domain.Enums;

public enum TramiteTipo
{
    Vacaciones = 1,
    Finiquito = 2,
    Constancia = 3,
    PermisoEspecial = 4,
    PrestamoUtilidades = 5,
    Otro = 6
}

public enum TramiteEstado
{
    Pendiente = 1,
    EnRevision = 2,
    Procesando = 3,
    Finalizado = 4,
    Rechazado = 5
}

public enum UserRole
{
    Colaborador = 1,
    Lider = 2,
    RRHH = 3,
    Admin = 4
}
