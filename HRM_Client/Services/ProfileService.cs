using HRM_Client.Models;
using System.Text.Json;

namespace HRM_Client.Services
{
    public class ProfileService
    {
        private readonly ApiClient _apiClient;

        public ProfileService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<UserProfileDto?> GetProfileAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync("/api/profile");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserProfileDto>(content, Utils.JsonOptions.DefaultOptions);
        }

        public async Task<bool> UpdateProfileAsync(UpdateProfileRequest request)
        {
            var client = _apiClient.GetClient();
            var response = await client.PutAsJsonAsync("/api/profile", request);

            return response.IsSuccessStatusCode;
        }
    }
}

