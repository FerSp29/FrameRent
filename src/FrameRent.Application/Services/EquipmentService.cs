using FrameRent.Application.DTOs.Equipment;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Entities;
using FrameRent.Domain.Exceptions;
using FrameRent.Domain.Interfaces;

namespace FrameRent.Application.Services;

public class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IRentalRepository _rentalRepository;

    public EquipmentService(IEquipmentRepository equipmentRepository, IRentalRepository rentalRepository)
    {
        _equipmentRepository = equipmentRepository;
        _rentalRepository = rentalRepository;
    }

    public async Task<IEnumerable<EquipmentResponseDto>> GetActiveCatalogAsync()
    {
        var items = await _equipmentRepository.GetAllActiveAsync();
        return items.Select(MapToDto);
    }

    public async Task<EquipmentResponseDto> GetByIdAsync(Guid id)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Equipment with ID '{id}' was not found.");

        if (!equipment.IsActive)
            throw new NotFoundException($"Equipment with ID '{id}' is inactive and unavailable.");

        return MapToDto(equipment);
    }

    public async Task<EquipmentAvailabilityDto> GetAvailabilityAsync(Guid id, DateOnly from, DateOnly to)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Equipment with ID '{id}' was not found.");

        if (!equipment.IsActive)
            throw new DomainValidationException("Cannot check availability for inactive equipment.");

        if (to <= from)
            throw new DomainValidationException("End date 'to' must be strictly after start date 'from'.");

        var reserved = await _rentalRepository.GetReservedUnitsInDateRangeAsync(id, from, to);
        var available = Math.Max(0, equipment.TotalStock - reserved);

        return new EquipmentAvailabilityDto(
            equipment.Id,
            equipment.Name,
            from,
            to,
            equipment.TotalStock,
            reserved,
            available);
    }

    public async Task<EquipmentResponseDto> CreateAsync(CreateEquipmentRequestDto request)
    {
        try
        {
            var equipment = new Equipment(
                request.Name,
                request.Category,
                request.Description,
                request.DailyRate,
                request.TotalStock);

            await _equipmentRepository.AddAsync(equipment);
            return MapToDto(equipment);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainValidationException(ex.Message);
        }
    }

    public async Task<EquipmentResponseDto> UpdateAsync(Guid id, UpdateEquipmentRequestDto request)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Equipment with ID '{id}' was not found.");

        try
        {
            equipment.Update(
                request.Name,
                request.Category,
                request.Description,
                request.DailyRate,
                request.TotalStock);

            await _equipmentRepository.UpdateAsync(equipment);
            return MapToDto(equipment);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainValidationException(ex.Message);
        }
    }

    public async Task<EquipmentResponseDto> DeactivateAsync(Guid id)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Equipment with ID '{id}' was not found.");

        var hasActiveRentals = await _equipmentRepository.HasApprovedOrDeliveredRentalsAsync(id);

        try
        {
            equipment.Deactivate(hasActiveRentals);
            await _equipmentRepository.UpdateAsync(equipment);
            return MapToDto(equipment);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }
    }

    private static EquipmentResponseDto MapToDto(Equipment e) =>
        new(e.Id, e.Name, e.Category, e.Description, e.DailyRate, e.TotalStock, e.IsActive);
}
