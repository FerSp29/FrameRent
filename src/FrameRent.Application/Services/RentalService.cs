using FrameRent.Application.DTOs.Rentals;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Entities;
using FrameRent.Domain.Enums;
using FrameRent.Domain.Exceptions;
using FrameRent.Domain.Interfaces;

namespace FrameRent.Application.Services;

public class RentalService : IRentalService
{
    private const int MaxActiveRentalsPerCustomer = 3;

    private readonly IRentalRepository _rentalRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public RentalService(IRentalRepository rentalRepository, IEquipmentRepository equipmentRepository)
    {
        _rentalRepository = rentalRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<RentalResponseDto> CreateRentalAsync(string customerId, CreateRentalRequestDto request)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(request.EquipmentId)
            ?? throw new NotFoundException($"Equipment with ID '{request.EquipmentId}' was not found.");

        if (!equipment.IsActive)
            throw new DomainValidationException("Selected equipment is inactive and cannot be rented.");

        // Validate customer limit: max 3 Pending or Approved rentals simultaneously
        var activeCustomerRentals = await _rentalRepository.CountPendingOrApprovedByCustomerAsync(customerId);
        if (activeCustomerRentals >= MaxActiveRentalsPerCustomer)
            throw new DomainValidationException($"Customer cannot have more than {MaxActiveRentalsPerCustomer} pending or approved rentals at the same time.");

        // Validate domain rules (dates, duration <= 15 days, quantity > 0, cost calculation)
        Rental rental;
        try
        {
            rental = new Rental(
                equipment.Id,
                customerId,
                request.Quantity,
                request.StartDate,
                request.EndDate,
                equipment.DailyRate);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainValidationException(ex.Message);
        }

        // Validate availability in date range (discounting Approved or Delivered overlapping rentals)
        var reservedUnits = await _rentalRepository.GetReservedUnitsInDateRangeAsync(
            equipment.Id,
            request.StartDate,
            request.EndDate);

        var availableUnits = equipment.TotalStock - reservedUnits;
        if (request.Quantity > availableUnits)
        {
            throw new DomainValidationException(
                $"Insufficient availability for '{equipment.Name}' between {request.StartDate:yyyy-MM-dd} and {request.EndDate:yyyy-MM-dd}. Available units: {Math.Max(0, availableUnits)}, requested: {request.Quantity}.");
        }

        await _rentalRepository.AddAsync(rental);

        var saved = await _rentalRepository.GetByIdAsync(rental.Id) ?? rental;
        return MapToDto(saved, equipment);
    }

    public async Task<IEnumerable<RentalResponseDto>> GetMyRentalsAsync(string customerId)
    {
        var rentals = await _rentalRepository.GetByCustomerIdAsync(customerId);
        return rentals.Select(r => MapToDto(r, r.Equipment));
    }

    public async Task<RentalResponseDto> GetRentalByIdAsync(Guid id, string currentUserId, bool isStaff)
    {
        var rental = await _rentalRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Rental with ID '{id}' was not found.");

        if (!isStaff && rental.CustomerId != currentUserId)
            throw new ForbiddenAccessException("You are not allowed to access another customer's rental.");

        return MapToDto(rental, rental.Equipment);
    }

    public async Task<RentalResponseDto> CancelRentalAsync(Guid id, string customerId)
    {
        var rental = await _rentalRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Rental with ID '{id}' was not found.");

        if (rental.CustomerId != customerId)
            throw new ForbiddenAccessException("You can only cancel your own rentals.");

        try
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            rental.Cancel(today);
            await _rentalRepository.UpdateAsync(rental);
            return MapToDto(rental, rental.Equipment);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainValidationException(ex.Message);
        }
    }

    public async Task<IEnumerable<RentalResponseDto>> GetAllRentalsAsync(string? status)
    {
        RentalStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<RentalStatus>(status, ignoreCase: true, out var statusEnum))
                throw new DomainValidationException($"Invalid rental status filter '{status}'. Valid values: Pending, Approved, Delivered, Returned, Rejected, Cancelled.");

            parsedStatus = statusEnum;
        }

        var rentals = await _rentalRepository.GetAllAsync(parsedStatus);
        return rentals.Select(r => MapToDto(r, r.Equipment));
    }

    public async Task<RentalResponseDto> ApproveRentalAsync(Guid id)
    {
        var rental = await _rentalRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Rental with ID '{id}' was not found.");

        var equipment = rental.Equipment ?? await _equipmentRepository.GetByIdAsync(rental.EquipmentId)
            ?? throw new NotFoundException($"Equipment with ID '{rental.EquipmentId}' was not found.");

        if (!equipment.IsActive)
            throw new DomainValidationException("Cannot approve a rental for an inactive equipment.");

        // Re-validate availability at approval time (Rule 6)
        var reservedUnits = await _rentalRepository.GetReservedUnitsInDateRangeAsync(
            rental.EquipmentId,
            rental.StartDate,
            rental.EndDate,
            excludeRentalId: rental.Id);

        var availableUnits = equipment.TotalStock - reservedUnits;
        if (rental.Quantity > availableUnits)
        {
            throw new ConflictException(
                $"Cannot approve rental: insufficient stock available between {rental.StartDate:yyyy-MM-dd} and {rental.EndDate:yyyy-MM-dd}. Available: {Math.Max(0, availableUnits)}, required: {rental.Quantity}.");
        }

        try
        {
            rental.Approve();
            await _rentalRepository.UpdateAsync(rental);
            return MapToDto(rental, equipment);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainValidationException(ex.Message);
        }
    }

    public async Task<RentalResponseDto> RejectRentalAsync(Guid id)
    {
        var rental = await _rentalRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Rental with ID '{id}' was not found.");

        try
        {
            rental.Reject();
            await _rentalRepository.UpdateAsync(rental);
            return MapToDto(rental, rental.Equipment);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainValidationException(ex.Message);
        }
    }

    public async Task<RentalResponseDto> DeliverRentalAsync(Guid id)
    {
        var rental = await _rentalRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Rental with ID '{id}' was not found.");

        try
        {
            rental.Deliver();
            await _rentalRepository.UpdateAsync(rental);
            return MapToDto(rental, rental.Equipment);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainValidationException(ex.Message);
        }
    }

    public async Task<RentalResponseDto> ReturnRentalAsync(Guid id)
    {
        var rental = await _rentalRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Rental with ID '{id}' was not found.");

        var equipment = rental.Equipment ?? await _equipmentRepository.GetByIdAsync(rental.EquipmentId)
            ?? throw new NotFoundException($"Equipment with ID '{rental.EquipmentId}' was not found.");

        try
        {
            rental.Return(DateTime.UtcNow, equipment.DailyRate);
            await _rentalRepository.UpdateAsync(rental);
            return MapToDto(rental, equipment);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainValidationException(ex.Message);
        }
    }

    private static RentalResponseDto MapToDto(Rental r, Equipment? e) =>
        new(
            r.Id,
            r.EquipmentId,
            e?.Name ?? string.Empty,
            e?.Category ?? string.Empty,
            e?.DailyRate ?? 0,
            r.CustomerId,
            r.Quantity,
            r.StartDate,
            r.EndDate,
            r.EndDate.DayNumber - r.StartDate.DayNumber,
            r.TotalCost,
            r.LateFee,
            r.Status.ToString(),
            r.ActualReturnDate);
}
