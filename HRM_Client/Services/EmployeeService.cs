using HRM_Client.Models;
using System.Text.Json;

namespace HRM_Client.Services
{
    public class EmployeeService
    {
        private readonly ApiClient _apiClient;

        public EmployeeService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<EmployeeDashboardStatsResponse?> GetDashboardStatsAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync("/api/employee/dashboard/stats");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EmployeeDashboardStatsResponse>(content, Utils.JsonOptions.DefaultOptions);
        }

        public async Task<(bool Success, string Message, bool IsOnTime)> CheckInAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.PostAsync("/api/employee/check-in", null);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(errorContent, Utils.JsonOptions.DefaultOptions);
                var message = errorObj.TryGetProperty("message", out var msg) ? msg.GetString() : "Check-in failed";
                return (false, message ?? "Check-in failed", false);
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content, Utils.JsonOptions.DefaultOptions);
            var successMessage = result.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Check-in successful";
            var isOnTime = result.TryGetProperty("isOnTime", out var onTimeProp) && onTimeProp.GetBoolean();

            return (true, successMessage ?? "Check-in successful", isOnTime);
        }

        public async Task<(bool Success, string Message, bool IsOnTime)> CheckOutAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.PostAsync("/api/employee/check-out", null);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(errorContent, Utils.JsonOptions.DefaultOptions);
                var message = errorObj.TryGetProperty("message", out var msg) ? msg.GetString() : "Check-out failed";
                return (false, message ?? "Check-out failed", false);
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content, Utils.JsonOptions.DefaultOptions);
            var successMessage = result.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Check-out successful";
            var isOnTime = result.TryGetProperty("isOnTime", out var onTimeProp) && onTimeProp.GetBoolean();

            return (true, successMessage ?? "Check-out successful", isOnTime);
        }

        public async Task<IncomeStatisticsResponse?> GetIncomeStatisticsAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync("/api/employee/income");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IncomeStatisticsResponse>(content, Utils.JsonOptions.DefaultOptions);
        }

        public async Task<AttendanceHistoryResponse?> GetAttendanceHistoryAsync(string period)
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync($"/api/employee/attendance/history?period={period}");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AttendanceHistoryResponse>(content, Utils.JsonOptions.DefaultOptions);
        }

        public async Task<(AttendanceRecordDto? Record, string? ErrorMessage)> SearchAttendanceByDateAsync(DateTime date)
        {
            var client = _apiClient.GetClient();
            var dateStr = date.ToString("yyyy-MM-dd");
            var response = await client.GetAsync($"/api/employee/attendance/search?date={dateStr}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(errorContent, Utils.JsonOptions.DefaultOptions);
                var message = errorObj.TryGetProperty("message", out var msg) ? msg.GetString() : "Not found";
                return (null, message);
            }

            if (!response.IsSuccessStatusCode)
                return (null, "Failed to search attendance");

            var content = await response.Content.ReadAsStringAsync();
            var record = JsonSerializer.Deserialize<AttendanceRecordDto>(content, Utils.JsonOptions.DefaultOptions);
            return (record, null);
        }

        public async Task<RequestListResponse?> GetMyRequestsAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync("/api/employee/requests");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RequestListResponse>(content, Utils.JsonOptions.DefaultOptions);
        }

        public async Task<(bool Success, string Message)> CreateRequestAsync(CreateRequestDto request)
        {
            var client = _apiClient.GetClient();
            // Convert to API format (Type should be enum value, not string)
            var apiRequest = new
            {
                type = request.Type,
                startDate = request.StartDate,
                endDate = request.EndDate,
                content = request.Content
            };
            var json = JsonSerializer.Serialize(apiRequest, Utils.JsonOptions.DefaultOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/employee/requests", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(errorContent, Utils.JsonOptions.DefaultOptions);
                var message = errorObj.TryGetProperty("message", out var msg) ? msg.GetString() : "Failed to create request";
                return (false, message ?? "Failed to create request");
            }

            var successContent = await response.Content.ReadAsStringAsync();
            var successObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(successContent, Utils.JsonOptions.DefaultOptions);
            var successMessage = successObj.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Request created successfully";
            return (true, successMessage ?? "Request created successfully");
        }
    }
}

