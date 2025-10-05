namespace HRM_UI.Dtos.Auth;

/// <summary>
/// DTO for login request
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}