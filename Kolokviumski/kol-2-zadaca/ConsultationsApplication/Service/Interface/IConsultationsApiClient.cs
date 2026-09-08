namespace Service.Interface;

public interface IConsultationsApiClient<T> where T : class
{
    Task<T> GetAllConsultationsModifiedSinceAsync(DateTime dateLastModified);
}