namespace Service.Interfaces;

public interface IFileUploadService
{
    public Task<string> UploadFileAsync(
        byte[] fileBytes,
        string originalFileName,
        string folder = "events"
    );
}