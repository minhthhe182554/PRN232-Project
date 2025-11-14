using HRM_Client.Models;
using HRM_Client.Utils;
using System.Text;
using System.Text.Json;

namespace HRM_Client.Services
{
    public class SalaryScaleService
    {
        private readonly ApiClient _apiClient;

        public SalaryScaleService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<SalaryScaleResponse>?> GetAllAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync("/api/salary-scales");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<SalaryScaleResponse>>(content, JsonOptions.DefaultOptions);
        }

        public async Task<SalaryScaleResponse?> UpdateAsync(UpdateSalaryScaleRequest request)
        {
            var client = _apiClient.GetClient();
            var json = JsonSerializer.Serialize(request, JsonOptions.DefaultOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/salary-scales/{request.Id}", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SalaryScaleResponse>(responseContent, JsonOptions.DefaultOptions);
        }
    }
}

