using Service.Interface;

namespace Service.Implementation;

public class FileUploadService : IFileUploadService
{
    public async Task<string> UploadFileAsync(byte[] fileBytes, string originalFileName, string folder = "cancellations")
    {
        var uploadsFolder = Path.Combine("wwwroot", "uploads", folder);
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(originalFileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await File.WriteAllBytesAsync(filePath, fileBytes);

        return $"/uploads/{folder}/{fileName}";
    }
}