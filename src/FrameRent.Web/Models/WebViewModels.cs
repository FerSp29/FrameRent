using System.ComponentModel.DataAnnotations;
using FrameRent.Application.DTOs.Equipment;

namespace FrameRent.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class EquipmentDetailViewModel
{
    public EquipmentResponseDto Equipment { get; set; } = null!;
    public DateOnly CheckFrom { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
    public DateOnly CheckTo { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4));
    public EquipmentAvailabilityDto? Availability { get; set; }
    public int Quantity { get; set; } = 1;
}
