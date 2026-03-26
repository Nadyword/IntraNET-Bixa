namespace IntranetCorp.Infrastructure.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Guid userId, Guid tramiteId, Stream fileStream, string fileName);
    Task<(Stream stream, string contentType)> GetFileAsync(string filePath);
    Task DeleteFileAsync(string filePath);
}

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService(string basePath)
    {
        _basePath = basePath;
        EnsureDirectoryExists(_basePath);
    }

    public async Task<string> SaveFileAsync(Guid userId, Guid tramiteId, Stream fileStream, string fileName)
    {
        var userDir = Path.Combine(_basePath, userId.ToString());
        var tramiteDir = Path.Combine(userDir, tramiteId.ToString());

        EnsureDirectoryExists(userDir);
        EnsureDirectoryExists(tramiteDir);

        var filePath = Path.Combine(tramiteDir, fileName);

        using (var fileFS = File.Create(filePath))
        {
            await fileStream.CopyToAsync(fileFS);
        }

        return filePath;
    }

    public async Task<(Stream stream, string contentType)> GetFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Archivo no encontrado: {filePath}");

        var stream = File.OpenRead(filePath);
        var contentType = GetContentType(filePath);

        return await Task.FromResult((stream, contentType));
    }

    public Task DeleteFileAsync(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private string GetContentType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }
}
