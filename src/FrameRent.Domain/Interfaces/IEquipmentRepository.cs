using FrameRent.Domain.Entities;

namespace FrameRent.Domain.Interfaces;

public interface IEquipmentRepository
{
    Task<IEnumerable<Equipment>> GetAllActiveAsync();
    Task<IEnumerable<Equipment>> GetAllAsync();
    Task<Equipment?> GetByIdAsync(Guid id);
    Task AddAsync(Equipment equipment);
    Task UpdateAsync(Equipment equipment);
    Task<bool> HasApprovedOrDeliveredRentalsAsync(Guid equipmentId);
}
