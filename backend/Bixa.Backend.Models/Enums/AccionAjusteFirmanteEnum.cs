namespace Bixa.Backend.Models.Enums;

/// <summary>
/// Tipo de diferencia que un ajuste guarda respecto de la cadena de firmantes que devuelve Profit.
/// El ajuste no almacena la lista completa: solo lo que se apartó de la jerarquía de supervisores.
/// </summary>
public enum AccionAjusteFirmanteEnum
{
    /// <summary>Firmante que no pertenece a la cadena de Profit y se inserta en la posición <c>Orden</c>.</summary>
    Agregado = 1,

    /// <summary>Firmante de la cadena de Profit que no debe firmar.</summary>
    Excluido = 2,

    /// <summary>Firmante de la cadena de Profit que se mueve a la posición <c>Orden</c>.</summary>
    Reubicado = 3,
}
