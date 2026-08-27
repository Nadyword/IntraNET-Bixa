namespace Bixa.Backend.Models.DTOs.FAQsDTO
{
    public class FAQsDTO
    {
        public int Id { get; set; }
        public required string Question { get; set; } = null!;
        public required string Response { get; set; } = null!;

        /// <summary>
        /// Posición en la que se muestra la pregunta. Menor valor = se muestra primero.
        /// Si se envía 0 o menos al crear, la pregunta se coloca al final.
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}