using HRM_Client.Models;
using System.Text;
using System.Text.Json;

namespace HRM_Client.Services
{
    public class ManagerService
    {
        private readonly ApiClient _apiClient;

        public ManagerService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ManagerDashboardStatsResponse?> GetDashboardStatsAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync("/api/manager/dashboard/stats");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ManagerDashboardStatsResponse>(content, Utils.JsonOptions.DefaultOptions);
        }

        public async Task<(bool Success, string Message, bool IsOnTime)> CheckInAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.PostAsync("/api/manager/check-in", null);

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
            var response = await client.PostAsync("/api/manager/check-out", null);

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

        public async Task<UserList?> GetDepartmentEmployeesAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync("/api/manager/employees");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserList>(content, Utils.JsonOptions.DefaultOptions);
        }

        public async Task<AttendanceHistoryResponse?> GetAttendanceHistoryAsync(string period)
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync($"/api/manager/attendance/history?period={period}");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AttendanceHistoryResponse>(content, Utils.JsonOptions.DefaultOptions);
        }

        public async Task<(AttendanceRecordDto? Record, string? ErrorMessage)> SearchAttendanceByDateAsync(DateTime date)
        {
            var client = _apiClient.GetClient();
            var dateStr = date.ToString("yyyy-MM-dd");
            var response = await client.GetAsync($"/api/manager/attendance/search?date={dateStr}");

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

        public async Task<RequestListResponse?> GetDepartmentRequestsAsync(string? type = null)
        {
            var client = _apiClient.GetClient();
            var url = "/api/manager/requests";
            if (!string.IsNullOrEmpty(type))
            {
                url += $"?type={type}";
            }
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RequestListResponse>(content, Utils.JsonOptions.DefaultOptions);
        }

        public async Task<(bool Success, string Message)> UpdateRequestStatusAsync(int requestId, string status)
        {
            var client = _apiClient.GetClient();
            var request = new UpdateRequestStatusRequest
            {
                RequestId = requestId,
                Status = status
            };

            var json = JsonSerializer.Serialize(request, Utils.JsonOptions.DefaultOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"/api/manager/requests/{requestId}/status", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(errorContent, Utils.JsonOptions.DefaultOptions);
                var message = errorObj.TryGetProperty("message", out var msg) ? msg.GetString() : "Failed to update request status";
                return (false, message ?? "Failed to update request status");
            }

            var successContent = await response.Content.ReadAsStringAsync();
            var successObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(successContent, Utils.JsonOptions.DefaultOptions);
            var successMessage = successObj.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Request updated successfully";
            return (true, successMessage ?? "Request updated successfully");
        }

        public async Task<IncomeStatisticsResponse?> GetIncomeStatisticsAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync("/api/manager/income");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IncomeStatisticsResponse>(content, Utils.JsonOptions.DefaultOptions);
        }
    }
}

