using HRM_Client.Models;
using HRM_Client.Utils;
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
    }
}

