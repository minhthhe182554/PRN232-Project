using HRM_Client.Models;
using HRM_Client.Utils;
using System.Text;
using System.Text.Json;

namespace HRM_Client.Services
{
    public class PolicyService
    {
        private readonly ApiClient _apiClient;

        public PolicyService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PolicyResponse?> GetPolicyAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync("/api/policies");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolicyResponse>(content, JsonOptions.DefaultOptions);
        }

        public async Task<PolicyResponse?> UpdatePolicyAsync(UpdatePolicyRequest request)
        {
            var client = _apiClient.GetClient();
            var json = JsonSerializer.Serialize(request, JsonOptions.DefaultOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/policies/{request.Id}", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolicyResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}

