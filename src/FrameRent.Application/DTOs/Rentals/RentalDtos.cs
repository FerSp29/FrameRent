namespace FrameRent.Application.DTOs.Rentals;

public record CreateRentalRequestDto(
    Guid EquipmentId,
    int Quantity,
    DateOnly StartDate,
    DateOnly EndDate);

public record RentalResponseDto(
    Guid Id,
    Guid EquipmentId,
    string EquipmentName,
    string EquipmentCategory,
    decimal DailyRate,
    string CustomerId,
    int Quantity,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalDays,
    decimal TotalCost,
    decimal LateFee,
    string Status,
    DateTime? ActualReturnDate);
