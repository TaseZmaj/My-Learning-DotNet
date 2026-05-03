namespace Service.Interfaces;

using EventsManagement.Domain.Entities;

public interface IReservationService
{
    Task<Reservation> InsertAsync(Guid eventId, string userId);
    Task<Reservation> UpdateAsync(Guid reservationId, Guid eventId);
    Task<Reservation> ConfirmAsync(Guid reservationId);
    Task<List<Reservation>>  GetAllAsync();
    Task<Reservation?> GetByIdAsync(Guid id);
    Task<Reservation> GetByIdNotNullAsync(Guid id);
    Task<Reservation> DeleteByIdAsync(Guid id);
    Task<List<Reservation>> GetAllByDateReservedSince(DateTime date);
    Task<Reservation> ExpireAsync(Reservation reservation);
}