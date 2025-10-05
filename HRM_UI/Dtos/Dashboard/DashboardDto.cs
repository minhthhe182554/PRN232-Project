namespace HRM_UI.Dtos.Dashboard;

/// <summary>
/// Simple DTO for dashboard response from API
/// </summary>
public class DashboardDto
{
    public string Role { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public string Message { get; set; } = string.Empty;
}
