using FrameRent.Application.DTOs.Equipment;

namespace FrameRent.Application.Interfaces;

public interface IEquipmentService
{
    Task<IEnumerable<EquipmentResponseDto>> GetActiveCatalogAsync();
    Task<EquipmentResponseDto> GetByIdAsync(Guid id);
    Task<EquipmentAvailabilityDto> GetAvailabilityAsync(Guid id, DateOnly from, DateOnly to);
    Task<EquipmentResponseDto> CreateAsync(CreateEquipmentRequestDto request);
    Task<EquipmentResponseDto> UpdateAsync(Guid id, UpdateEquipmentRequestDto request);
    Task<EquipmentResponseDto> DeactivateAsync(Guid id);
}
