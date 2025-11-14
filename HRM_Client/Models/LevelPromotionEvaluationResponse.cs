namespace HRM_Client.Models
{
    public class LevelPromotionEvaluationResponse
    {
        public bool HasRecommendation { get; set; }
        public int? RecommendedUserId { get; set; }
        public string? RecommendedUserName { get; set; }
        public int? CurrentLevel { get; set; }
        public int? RecommendedLevel { get; set; }
        public string? Reason { get; set; }
    }

    public class PromoteLevelRequest
    {
        public int UserId { get; set; }
    }

    public class PromoteLevelResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int OldLevel { get; set; }
        public int NewLevel { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

