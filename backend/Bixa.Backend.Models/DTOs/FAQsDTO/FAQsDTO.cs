namespace Bixa.Backend.Models.DTOs.FAQsDTO
{
    public class FAQsDTO
    {
        public int Id { get; set; }
        public required string Question { get; set; } = null!;
        public required string Response { get; set; } = null!;
    }
}