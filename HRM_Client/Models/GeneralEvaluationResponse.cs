using System.Text.Json.Serialization;

namespace HRM_Client.Models
{
    public class GeneralEvaluationResponse
    {
        [JsonPropertyName("overallEvaluation")]
        public OverallEvaluation OverallEvaluation { get; set; } = new();

        [JsonPropertyName("topPerformers")]
        public List<EmployeeEvaluation> TopPerformers { get; set; } = new();

        [JsonPropertyName("needsImprovement")]
        public List<EmployeeEvaluationWithIssues> NeedsImprovement { get; set; } = new();

        [JsonPropertyName("departmentAnalysis")]
        public List<DepartmentAnalysis> DepartmentAnalysis { get; set; } = new();
    }

    public class OverallEvaluation
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("averageScore")]
        public double AverageScore { get; set; }

        [JsonPropertyName("strengths")]
        public List<string> Strengths { get; set; } = new();

        [JsonPropertyName("weaknesses")]
        public List<string> Weaknesses { get; set; } = new();

        [JsonPropertyName("recommendations")]
        public List<string> Recommendations { get; set; } = new();
    }

    public class EmployeeEvaluation
    {
        [JsonPropertyName("employeeId")]
        public int EmployeeId { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("highlights")]
        public List<string> Highlights { get; set; } = new();
    }

    public class EmployeeEvaluationWithIssues : EmployeeEvaluation
    {
        [JsonPropertyName("issues")]
        public List<string> Issues { get; set; } = new();
    }

    public class DepartmentAnalysis
    {
        [JsonPropertyName("departmentName")]
        public string DepartmentName { get; set; } = string.Empty;

        [JsonPropertyName("averageScore")]
        public double AverageScore { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;
    }
}

