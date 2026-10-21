using FrameRent.Domain.Entities;
using FrameRent.Domain.Enums;

namespace FrameRent.Domain.Interfaces;

public interface IRentalRepository
{
    Task<Rental?> GetByIdAsync(Guid id);
    Task<IEnumerable<Rental>> GetByCustomerIdAsync(string customerId);
    Task<IEnumerable<Rental>> GetAllAsync(RentalStatus? status = null);
    Task<int> GetReservedUnitsInDateRangeAsync(Guid equipmentId, DateOnly from, DateOnly to, Guid? excludeRentalId = null);
    Task<int> CountPendingOrApprovedByCustomerAsync(string customerId);
    Task AddAsync(Rental rental);
    Task UpdateAsync(Rental rental);
}
