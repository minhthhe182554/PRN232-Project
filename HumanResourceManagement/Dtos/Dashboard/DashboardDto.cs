namespace HumanResourceManagement.Dtos.Dashboard;

/// <summary>
/// Simple DTO for dashboard - returns basic user info
/// </summary>
public class DashboardDto
{
    public string Role { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
}
