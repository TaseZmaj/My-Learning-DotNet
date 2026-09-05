using Service.Interface;

namespace Service.Implementation;

public class FileUploadService : IFileUploadService
{
    public Task<string> UploadFileAsync(byte[] fileBytes, string originalFileName, string folder = "cancellations")
    {
        throw new NotImplementedException();
    }
}