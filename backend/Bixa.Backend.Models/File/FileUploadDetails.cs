namespace Bixa.Backend.Models.File;

public class FileUploadDetails
{
    public int Id { get; set; }
    public required string DriveId { get; set; }
    public required string FileName { get; set; }
}