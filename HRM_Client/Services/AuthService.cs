using HRM_Client.Models;
using HRM_Client.Utils;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HRM_Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(LoginResponse? response, string? errorMessage)> LoginAsync(LoginRequest request)
        {
            var json = JsonSerializer.Serialize(request, JsonOptions.DefaultOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                // Try to get error message from API
                try
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent, JsonOptions.DefaultOptions);
                    if (errorObj.TryGetProperty("message", out var messageElement))
                    {
                        return (null, messageElement.GetString());
                    }
                }
                catch { }
                
                return (null, null);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, JsonOptions.DefaultOptions);
            return (loginResponse, null);
        }

        public void SaveToken(string token)
        {
            var context = _httpContextAccessor.HttpContext;
            context?.Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set true in production with HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });
        }

        public string? GetToken()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request.Cookies["AuthToken"];
        }

        public void Logout()
        {
            var context = _httpContextAccessor.HttpContext;
            context?.Response.Cookies.Delete("AuthToken");
        }

        public async Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return null;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(request, JsonOptions.DefaultOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/register", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RegisterResponse>(responseContent, JsonOptions.DefaultOptions);
        }
    }
}

