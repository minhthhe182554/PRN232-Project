using HRM_Client.Models;
using System.Text.Json;

namespace HRM_Client.Services
{
    public class PerformanceEvaluationService
    {
        private readonly ApiClient _apiClient;

        public PerformanceEvaluationService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<string?> GetGeneralEvaluationAsync()
        {
            try
            {
                var client = _apiClient.GetClient();
                // Ensure timeout is set for this request
                if (client.Timeout.TotalMinutes < 10)
                {
                    client.Timeout = TimeSpan.FromMinutes(10);
                }
                
                var response = await client.PostAsync("/api/admin/performance/general-evaluation", null);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content, Utils.JsonOptions.DefaultOptions);
                
                if (result.TryGetProperty("evaluation", out var evaluationProperty))
                {
                    return evaluationProperty.GetString();
                }
                
                return null;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new TimeoutException("Request timed out. Please try again.");
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("Request was cancelled or timed out.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get evaluation: {ex.Message}", ex);
            }
        }

        public async Task<LevelPromotionEvaluationResponse?> GetLevelPromotionEvaluationAsync(string role)
        {
            try
            {
                var client = _apiClient.GetClient();
                if (client.Timeout.TotalMinutes < 10)
                {
                    client.Timeout = TimeSpan.FromMinutes(10);
                }

                var jsonContent = JsonSerializer.Serialize(role, Utils.JsonOptions.DefaultOptions);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync("/api/admin/performance/level-promotion-evaluation", content);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<LevelPromotionEvaluationResponse>(responseContent, Utils.JsonOptions.DefaultOptions);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new TimeoutException("Request timed out. Please try again.");
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("Request was cancelled or timed out.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get level promotion evaluation: {ex.Message}", ex);
            }
        }

        public async Task<PromoteLevelResponse?> PromoteLevelAsync(int userId)
        {
            try
            {
                var client = _apiClient.GetClient();
                var request = new PromoteLevelRequest { UserId = userId };
                var jsonContent = JsonSerializer.Serialize(request, Utils.JsonOptions.DefaultOptions);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync("/api/admin/performance/promote-level", content);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PromoteLevelResponse>(responseContent, Utils.JsonOptions.DefaultOptions);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to promote level: {ex.Message}", ex);
            }
        }
    }
}

