using HRM_Client.Models;
using System.Text.Json;

namespace HRM_Client.Services
{
    public class DashboardService
    {
        private readonly ApiClient _apiClient;

        public DashboardService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<DashboardStatsResponse?> GetAdminStatsAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync("/api/dashboard/stats");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DashboardStatsResponse>(content, Utils.JsonOptions.DefaultOptions);
        }
    }
}

