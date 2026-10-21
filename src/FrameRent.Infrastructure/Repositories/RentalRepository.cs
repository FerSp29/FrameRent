using FrameRent.Domain.Entities;
using FrameRent.Domain.Enums;
using FrameRent.Domain.Interfaces;
using FrameRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrameRent.Infrastructure.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly ApplicationDbContext _context;

    public RentalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Rental?> GetByIdAsync(Guid id)
    {
        return await _context.Rentals
            .Include(r => r.Equipment)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Rental>> GetByCustomerIdAsync(string customerId)
    {
        return await _context.Rentals
            .AsNoTracking()
            .Include(r => r.Equipment)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Rental>> GetAllAsync(RentalStatus? status = null)
    {
        var query = _context.Rentals
            .AsNoTracking()
            .Include(r => r.Equipment)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        return await query
            .OrderByDescending(r => r.StartDate)
            .ToListAsync();
    }

    public async Task<int> GetReservedUnitsInDateRangeAsync(Guid equipmentId, DateOnly from, DateOnly to, Guid? excludeRentalId = null)
    {
        // Overlapping range: r.StartDate < to && r.EndDate > from (or inclusive intersection r.StartDate <= to && r.EndDate >= from)
        var query = _context.Rentals.Where(r =>
            r.EquipmentId == equipmentId &&
            (r.Status == RentalStatus.Approved || r.Status == RentalStatus.Delivered) &&
            r.StartDate <= to &&
            r.EndDate >= from);

        if (excludeRentalId.HasValue)
        {
            query = query.Where(r => r.Id != excludeRentalId.Value);
        }

        return await query.SumAsync(r => r.Quantity);
    }

    public async Task<int> CountPendingOrApprovedByCustomerAsync(string customerId)
    {
        return await _context.Rentals.CountAsync(r =>
            r.CustomerId == customerId &&
            (r.Status == RentalStatus.Pending || r.Status == RentalStatus.Approved));
    }

    public async Task AddAsync(Rental rental)
    {
        await _context.Rentals.AddAsync(rental);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Rental rental)
    {
        _context.Rentals.Update(rental);
        await _context.SaveChangesAsync();
    }
}
