namespace FrameRent.Application.DTOs.Auth;

public record RegisterRequestDto(
    string Email,
    string Password,
    string FullName = "");

public record LoginRequestDto(
    string Email,
    string Password);

public record AuthResponseDto(
    string Token,
    string Email,
    string UserId,
    string Role,
    DateTime ExpiresAt);
