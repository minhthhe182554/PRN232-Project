using HRM_Client.Models;
using HRM_Client.Utils;
using System.Text;
using System.Text.Json;

namespace HRM_Client.Services
{
    public class DepartmentService
    {
        private readonly ApiClient _apiClient;

        public DepartmentService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<DepartmentResponse>?> GetAllAsync()
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync("/api/departments");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DepartmentResponse>>(content, JsonOptions.DefaultOptions);
        }

        public async Task<DepartmentResponse?> UpdateNameAsync(UpdateDepartmentNameRequest request)
        {
            var client = _apiClient.GetClient();
            var json = JsonSerializer.Serialize(request, JsonOptions.DefaultOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/departments/{request.Id}/name", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DepartmentResponse>(responseContent, JsonOptions.DefaultOptions);
        }

        public async Task<DepartmentResponse?> UpdateManagerAsync(UpdateDepartmentManagerRequest request)
        {
            var client = _apiClient.GetClient();
            var json = JsonSerializer.Serialize(request, JsonOptions.DefaultOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/departments/{request.DepartmentId}/manager", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DepartmentResponse>(responseContent, JsonOptions.DefaultOptions);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var client = _apiClient.GetClient();
            var response = await client.DeleteAsync($"/api/departments/{id}");

            return response.IsSuccessStatusCode;
        }

        public async Task<List<EmployeeSearchResult>?> SearchEmployeesAsync(string username, int? departmentId = null)
        {
            var client = _apiClient.GetClient();
            var url = $"/api/departments/search-employees?username={Uri.EscapeDataString(username)}";
            if (departmentId.HasValue)
            {
                url += $"&departmentId={departmentId.Value}";
            }
            
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<EmployeeSearchResult>>(content, JsonOptions.DefaultOptions);
        }

        public async Task<List<EmployeeSearchResult>?> GetEmployeesByDepartmentAsync(int departmentId)
        {
            var client = _apiClient.GetClient();
            var response = await client.GetAsync($"/api/departments/{departmentId}/employees");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<EmployeeSearchResult>>(content, JsonOptions.DefaultOptions);
        }

        public async Task<DepartmentResponse?> CreateAsync(CreateDepartmentRequest request)
        {
            var client = _apiClient.GetClient();
            var json = JsonSerializer.Serialize(request, JsonOptions.DefaultOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/departments", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DepartmentResponse>(responseContent, JsonOptions.DefaultOptions);
        }
    }
}

