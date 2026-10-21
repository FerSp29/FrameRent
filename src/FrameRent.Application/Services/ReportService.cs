using FrameRent.Application.DTOs.Equipment;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Enums;
using FrameRent.Domain.Interfaces;

namespace FrameRent.Application.Services;

public class ReportService : IReportService
{
    private readonly IEquipmentRepository _equipmentRepository;

    public ReportService(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    public async Task<IEnumerable<TopEquipmentReportDto>> GetTopEquipmentAsync()
    {
        var allEquipment = await _equipmentRepository.GetAllAsync();

        var report = allEquipment
            .Select(e =>
            {
                var validRentals = e.Rentals
                    .Where(r => r.Status != RentalStatus.Rejected && r.Status != RentalStatus.Cancelled)
                    .ToList();

                return new TopEquipmentReportDto(
                    e.Id,
                    e.Name,
                    e.Category,
                    e.DailyRate,
                    TotalRentalsCount: validRentals.Count,
                    TotalUnitsRented: validRentals.Sum(r => r.Quantity),
                    TotalRevenue: validRentals.Sum(r => r.TotalCost + r.LateFee));
            })
            .OrderByDescending(r => r.TotalRentalsCount)
            .ThenByDescending(r => r.TotalUnitsRented)
            .ToList();

        return report;
    }
}
