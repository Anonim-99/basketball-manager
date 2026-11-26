using System.ComponentModel.DataAnnotations;

namespace HoopsApi.Auth;

public class RegisterRequest
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string Email,
    string DisplayName,
    string[] Roles);

public record MeResponse(
    string Id,
    string Email,
    string DisplayName,
    bool IsApproved,
    string[] Roles);

public record PendingUserDto(
    string Id,
    string Email,
    string DisplayName,
    DateTime CreatedAt);