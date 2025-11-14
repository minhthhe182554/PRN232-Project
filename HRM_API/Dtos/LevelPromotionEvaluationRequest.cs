using System.Text.Json.Serialization;

namespace HRM_API.Dtos
{
    public class LevelPromotionEvaluationRequest
    {
        [JsonPropertyName("evaluationPeriod")]
        public string EvaluationPeriod { get; set; } = "30 days";

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("totalCandidates")]
        public int TotalCandidates { get; set; }

        [JsonPropertyName("candidates")]
        public List<EmployeePerformanceData> Candidates { get; set; } = new();
    }

    public class LevelPromotionEvaluationResponse
    {
        [JsonPropertyName("hasRecommendation")]
        public bool HasRecommendation { get; set; }

        [JsonPropertyName("recommendedUserId")]
        public int? RecommendedUserId { get; set; }

        [JsonPropertyName("recommendedUserName")]
        public string? RecommendedUserName { get; set; }

        [JsonPropertyName("currentLevel")]
        public int? CurrentLevel { get; set; }

        [JsonPropertyName("recommendedLevel")]
        public int? RecommendedLevel { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }

    public class PromoteLevelRequest
    {
        [JsonPropertyName("userId")]
        public int UserId { get; set; }
    }

    public class PromoteLevelResponse
    {
        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("oldLevel")]
        public int OldLevel { get; set; }

        [JsonPropertyName("newLevel")]
        public int NewLevel { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}

