using HRM_Client.Models;
using HRM_Client.Utils;
using System.Text;
using System.Text.Json;

namespace HRM_Client.Services
{
    public class UserService
    {
        private readonly ApiClient _apiClient;

        public UserService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<UserList?> GetAllUsersAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync("/api/users");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserList>(content, JsonOptions.DefaultOptions);
        }

        public async Task<BanUserResponse?> BanUserAsync(BanUserRequest request)
        {
            var client = _apiClient.GetClient();
            var json = JsonSerializer.Serialize(request, JsonOptions.DefaultOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PatchAsync("/api/users/ban", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BanUserResponse>(responseContent, JsonOptions.DefaultOptions);
        }

        public async Task<ResetPasswordResponse?> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var client = _apiClient.GetClient();
            var json = JsonSerializer.Serialize(request, JsonOptions.DefaultOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/users/reset-password", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ResetPasswordResponse>(responseContent, JsonOptions.DefaultOptions);
        }
    }
}

