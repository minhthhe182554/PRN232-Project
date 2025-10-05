namespace HRM_UI.Dtos.Auth;

/// <summary>
/// DTO for login response from API
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public UserDto? User { get; set; }
}
