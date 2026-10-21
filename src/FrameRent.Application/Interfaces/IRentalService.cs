using FrameRent.Application.DTOs.Rentals;

namespace FrameRent.Application.Interfaces;

public interface IRentalService
{
    Task<RentalResponseDto> CreateRentalAsync(string customerId, CreateRentalRequestDto request);
    Task<IEnumerable<RentalResponseDto>> GetMyRentalsAsync(string customerId);
    Task<RentalResponseDto> GetRentalByIdAsync(Guid id, string currentUserId, bool isStaff);
    Task<RentalResponseDto> CancelRentalAsync(Guid id, string customerId);
    Task<IEnumerable<RentalResponseDto>> GetAllRentalsAsync(string? status);
    Task<RentalResponseDto> ApproveRentalAsync(Guid id);
    Task<RentalResponseDto> RejectRentalAsync(Guid id);
    Task<RentalResponseDto> DeliverRentalAsync(Guid id);
    Task<RentalResponseDto> ReturnRentalAsync(Guid id);
}
