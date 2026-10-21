namespace FrameRent.Domain.Entities;

public class Equipment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal DailyRate { get; private set; }
    public int TotalStock { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<Rental> Rentals { get; private set; } = new List<Rental>();

    private Equipment() { }

    public Equipment(string name, string category, string description, decimal dailyRate, int totalStock)
    {
        Validate(name, category, dailyRate, totalStock);

        Name = name.Trim();
        Category = category.Trim();
        Description = description?.Trim() ?? string.Empty;
        DailyRate = dailyRate;
        TotalStock = totalStock;
        IsActive = true;
    }

    public void Update(string name, string category, string description, decimal dailyRate, int totalStock)
    {
        Validate(name, category, dailyRate, totalStock);

        Name = name.Trim();
        Category = category.Trim();
        Description = description?.Trim() ?? string.Empty;
        DailyRate = dailyRate;
        TotalStock = totalStock;
    }

    public void Deactivate(bool hasActiveOrApprovedRentals)
    {
        if (hasActiveOrApprovedRentals)
            throw new InvalidOperationException("Cannot deactivate equipment with approved or delivered rentals.");

        IsActive = false;
    }

    private static void Validate(string name, string category, decimal dailyRate, int totalStock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Equipment name is required.");

        if (string.IsNullOrWhiteSpace(category))
            throw new InvalidOperationException("Equipment category is required.");

        if (dailyRate <= 0)
            throw new InvalidOperationException("Daily rate must be greater than zero.");

        if (totalStock < 0)
            throw new InvalidOperationException("Total stock cannot be negative.");
    }
}