namespace FrameRent.Application.DTOs.Equipment;

public record CreateEquipmentRequestDto(
    string Name,
    string Category,
    string Description,
    decimal DailyRate,
    int TotalStock);

public record UpdateEquipmentRequestDto(
    string Name,
    string Category,
    string Description,
    decimal DailyRate,
    int TotalStock);

public record EquipmentResponseDto(
    Guid Id,
    string Name,
    string Category,
    string Description,
    decimal DailyRate,
    int TotalStock,
    bool IsActive);

public record EquipmentAvailabilityDto(
    Guid EquipmentId,
    string EquipmentName,
    DateOnly From,
    DateOnly To,
    int TotalStock,
    int ReservedUnits,
    int AvailableUnits);

public record TopEquipmentReportDto(
    Guid EquipmentId,
    string Name,
    string Category,
    decimal DailyRate,
    int TotalRentalsCount,
    int TotalUnitsRented,
    decimal TotalRevenue);
