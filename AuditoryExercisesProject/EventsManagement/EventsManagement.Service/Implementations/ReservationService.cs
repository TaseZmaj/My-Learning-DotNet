using Service.Interfaces;
using EventsManagement.Domain.Entities;
using EventsManagement.Domain.Enums;
using EventsManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Service.Implementations;

public class ReservationService : IReservationService
{
    private readonly IRepository<Reservation> _reservationRepository;

    public ReservationService(IRepository<Reservation> reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<List<Reservation>> GetAllAsync()
    {
        var result =  await _reservationRepository.GetAllAsync(
            selector: x => x,
            include: x => x.Include(r => r.Event)
                .Include(r => r.User));
        return result.ToList();
    }

    public async Task<Reservation?> GetByIdAsync(Guid id)
    {
        return await _reservationRepository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == id);
    }

    public async Task<List<Reservation>> GetAllByDateReservedSince(DateTime date)
    {
        var result = await _reservationRepository.GetAllAsync(
            selector: x => x,
            predicate: x => x.Status == ReservationStatus.Pending && x.ReservedAt < date);
        return result.ToList();
    }
    
    public async Task<Reservation> GetByIdNotNullAsync(Guid id)
    {
        var result = await GetByIdAsync(id);

        if (result == null)
        {
            throw new InvalidOperationException($"Reservation with id {id} not found");
        }
        
        return result;
    }
    
    public async Task<Reservation> InsertAsync(Guid eventId, string userId)
    {
        var reservation = new Reservation()
        {
            EventId = eventId,
            UserId = userId,
            ReservedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddDays(10),
            Status = ReservationStatus.Pending
        };
        
        return await _reservationRepository.InsertAsync(reservation);
    }

    public async Task<Reservation> UpdateAsync(Guid reservationId, Guid eventId)
    {
        var reservation = await GetByIdNotNullAsync(reservationId);
        
        reservation.EventId = eventId;
        
        return await _reservationRepository.UpdateAsync(reservation);
        
    }

    public async Task<Reservation> ConfirmAsync(Guid reservationId)
    {
        var reservation = await GetByIdNotNullAsync(reservationId);
        
        reservation.Status = ReservationStatus.Confirmed;
        
        return await _reservationRepository.UpdateAsync(reservation);
    }

    public async Task<Reservation> DeleteByIdAsync(Guid id)
    {
        var result = await GetByIdNotNullAsync(id);
        return await _reservationRepository.DeleteAsync(result);
    }
    
    public async Task<Reservation> ExpireAsync(Reservation reservation)
    {
        reservation.Status = ReservationStatus.Expired;
        return await _reservationRepository.UpdateAsync(reservation);
    }
}