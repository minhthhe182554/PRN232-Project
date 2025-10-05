namespace HumanResourceManagement.Dtos.Auth;

/// <summary>
/// DTO for login response
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public UserDto? User { get; set; }
}
