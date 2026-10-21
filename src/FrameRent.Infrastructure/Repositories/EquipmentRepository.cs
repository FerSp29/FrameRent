using FrameRent.Domain.Entities;
using FrameRent.Domain.Enums;
using FrameRent.Domain.Interfaces;
using FrameRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrameRent.Infrastructure.Repositories;

public class EquipmentRepository : IEquipmentRepository
{
    private readonly ApplicationDbContext _context;

    public EquipmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Equipment>> GetAllActiveAsync()
    {
        return await _context.Equipments
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Category)
            .ThenBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Equipment>> GetAllAsync()
    {
        return await _context.Equipments
            .AsNoTracking()
            .Include(e => e.Rentals)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<Equipment?> GetByIdAsync(Guid id)
    {
        return await _context.Equipments
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task AddAsync(Equipment equipment)
    {
        await _context.Equipments.AddAsync(equipment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Equipment equipment)
    {
        _context.Equipments.Update(equipment);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasApprovedOrDeliveredRentalsAsync(Guid equipmentId)
    {
        return await _context.Rentals.AnyAsync(r =>
            r.EquipmentId == equipmentId &&
            (r.Status == RentalStatus.Approved || r.Status == RentalStatus.Delivered));
    }
}
