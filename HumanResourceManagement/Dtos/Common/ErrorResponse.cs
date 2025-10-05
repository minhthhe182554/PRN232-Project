namespace HumanResourceManagement.Dtos.Common;

/// <summary>
/// Standard error response DTO
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}
