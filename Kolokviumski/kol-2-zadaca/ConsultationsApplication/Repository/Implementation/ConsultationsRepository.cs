using Domain.Models;
using EFCore.BulkExtensions;
using Repository.Interface;

namespace Repository.Implementation;

public class ConsultationsRepository : Repository<Consultation>, IConsultationsRepository
{
    public ConsultationsRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task BulkInsertOrUpdateAsync(List<Consultation> consultations)
    {
        await _context.BulkInsertOrUpdateAsync(consultations);
    }
}