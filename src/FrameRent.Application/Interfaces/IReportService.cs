using FrameRent.Application.DTOs.Equipment;

namespace FrameRent.Application.Interfaces;

public interface IReportService
{
    Task<IEnumerable<TopEquipmentReportDto>> GetTopEquipmentAsync();
}
