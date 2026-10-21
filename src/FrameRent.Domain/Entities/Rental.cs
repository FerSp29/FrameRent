using FrameRent.Domain.Enums;

namespace FrameRent.Domain.Entities;

public class Rental
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid EquipmentId { get; private set; }
    public Equipment Equipment { get; private set; } = null!;
    
    public string CustomerId { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal TotalCost { get; private set; }
    public decimal LateFee { get; private set; } = 0;
    public RentalStatus Status { get; private set; } = RentalStatus.Pending;
    public DateTime? ActualReturnDate { get; private set; }

    private Rental() { }

    public Rental(Guid equipmentId, string customerId, int quantity, DateOnly startDate, DateOnly endDate, decimal dailyRate)
    {
        if (startDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new InvalidOperationException("Start date cannot be in the past.");

        if (endDate <= startDate)
            throw new InvalidOperationException("End date must be strictly after start date.");

        int days = endDate.DayNumber - startDate.DayNumber;
        if (days > 15)
            throw new InvalidOperationException("Rentals cannot exceed 15 days.");

        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        EquipmentId = equipmentId;
        CustomerId = customerId;
        Quantity = quantity;
        StartDate = startDate;
        EndDate = endDate;
        TotalCost = days * dailyRate * quantity;
        Status = RentalStatus.Pending;
    }

    public void Approve()
    {
        if (Status != RentalStatus.Pending)
            throw new InvalidOperationException($"Cannot approve a rental in {Status} state.");
        Status = RentalStatus.Approved;
    }

    public void Reject()
    {
        if (Status != RentalStatus.Pending)
            throw new InvalidOperationException($"Cannot reject a rental in {Status} state.");
        Status = RentalStatus.Rejected;
    }

    public void Deliver()
    {
        if (Status != RentalStatus.Approved)
            throw new InvalidOperationException("Only approved rentals can be marked as delivered.");
        Status = RentalStatus.Delivered;
    }

    public void Return(DateTime returnTimestamp, decimal dailyRate)
    {
        if (Status != RentalStatus.Delivered)
            throw new InvalidOperationException("Only delivered rentals can be returned.");

        ActualReturnDate = returnTimestamp;
        Status = RentalStatus.Returned;

        DateOnly returnDate = DateOnly.FromDateTime(returnTimestamp);
        if (returnDate > EndDate)
        {
            int lateDays = returnDate.DayNumber - EndDate.DayNumber;
            LateFee = lateDays * dailyRate * Quantity * 1.5m;
        }
    }

    public void Cancel(DateOnly cancellationDate)
    {
        if (Status != RentalStatus.Pending && Status != RentalStatus.Approved)
            throw new InvalidOperationException("Only pending or approved rentals can be cancelled.");

        if (cancellationDate >= StartDate)
            throw new InvalidOperationException("Rentals can only be cancelled strictly before the start date.");

        Status = RentalStatus.Cancelled;
    }
}