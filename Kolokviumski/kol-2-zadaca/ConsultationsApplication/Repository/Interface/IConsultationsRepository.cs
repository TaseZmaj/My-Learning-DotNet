using Domain.Models;

namespace Repository.Interface;

public interface IConsultationsRepository
{
    Task BulkInsertOrUpdateAsync(List<Consultation> consultations);
}