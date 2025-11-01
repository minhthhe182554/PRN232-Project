using HRM_Client.Models;
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

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/login", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
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
    }
}

