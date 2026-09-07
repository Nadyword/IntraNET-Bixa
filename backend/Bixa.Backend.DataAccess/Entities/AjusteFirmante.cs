using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.DataAccess.Entities;

/// <summary>
/// Una diferencia entre la cadena de firmantes que Profit deriva de <c>snemple.supervisor</c> y la
/// que debe usarse realmente para las solicitudes de un empleado.
/// </summary>
/// <remarks>
/// Se guardan solo las diferencias, no la lista completa: así, cuando la jerarquía de Profit cambia,
/// los supervisores nuevos entran solos en la cadena y estos ajustes se siguen aplicando encima.
/// <para>
/// <see cref="FirmanteCi"/> no es clave foránea a <c>Users</c> a propósito: un firmante de la cadena
/// de Profit puede no tener usuario en la intranet y aun así necesitar ser excluido o reubicado. Los
/// firmantes <see cref="AccionAjusteFirmanteEnum.Agregado"/> sí se validan contra <c>Users</c> al
/// guardar, porque terminan en <see cref="Aprobacion.AprobadorCi"/>, que sí exige el usuario.
/// </para>
/// </remarks>
public class AjusteFirmante : BaseEntities
{
    /// <summary>Empleado dueño del ajuste: el que hace las solicitudes.</summary>
    public required string UserCi { get; set; }

    /// <summary>Firmante al que afecta la diferencia.</summary>
    public required string FirmanteCi { get; set; }

    /// <summary>Nombre del firmante al momento de guardar; evita reconsultar Profit para mostrarlo.</summary>
    public string? NombreFirmante { get; set; }

    public AccionAjusteFirmanteEnum Accion { get; set; }

    /// <summary>Posición (1 = primero) a la que va el firmante. Sin valor cuando fue excluido.</summary>
    public int? Orden { get; set; }

    /*-----------------------*/
    public virtual Users? User { get; set; }
}
