using FrameRent.Application.DTOs.Auth;

namespace FrameRent.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterCustomerAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}
