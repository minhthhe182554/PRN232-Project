using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HRM_API.Dtos;

namespace HRM_API.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _apiUrl;

        public GeminiService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key is not configured");
            _model = configuration["Gemini:Model"] ?? "gemini-2.5-flash";
            _apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10); // Increase timeout for large data processing
        }

        public async Task<string> EvaluatePerformanceAsync(GeneralEvaluationRequest request)
        {
            try
            {
                // Build prompt
                var systemMessage = BuildSystemPrompt();
                var userMessage = BuildUserPrompt(request);

                // Create request body
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = $"{systemMessage}\n\n{userMessage}"
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 8192
                    }
                };

                // Call Gemini API
                var response = await _httpClient.PostAsJsonAsync(_apiUrl, requestBody);
                
                // Handle rate limiting and other HTTP errors
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    throw new InvalidOperationException("Gemini API rate limit exceeded. Please try again in a few moments.");
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"Gemini API returned error: {response.StatusCode}. {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Length == 0)
                {
                    throw new InvalidOperationException("Invalid response from Gemini API: No candidates returned");
                }

                var candidate = geminiResponse.Candidates[0];
                if (candidate.Content?.Parts == null || candidate.Content.Parts.Length == 0)
                {
                    throw new InvalidOperationException("Invalid response from Gemini API: No content parts");
                }

                var text = candidate.Content.Parts[0].Text;
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException("Invalid response from Gemini API: Empty text");
                }

                return text;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new InvalidOperationException("Gemini API request timed out. Please try again.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new InvalidOperationException("Gemini API request was cancelled.", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to call Gemini API: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse Gemini API response: {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                throw new InvalidOperationException("Gemini API request timed out. Please try again.", ex);
            }
        }

        private string BuildSystemPrompt()
        {
            return @"You are a professional HR performance analyst. Analyze employee performance data for the last 7 days and provide a concise, well-formatted evaluation report.

Format your response as a structured text report with clear sections:
- Executive Summary
- Overall Performance Analysis
- Key Strengths
- Areas for Improvement
- Top Performers (mention names and highlights)
- Employees Needing Attention (mention names and issues)
- Department Performance Summary
- Recommendations

Use clear headings, bullet points, and organize information logically. Be professional, direct, and data-driven. Write in English. Keep the report concise and focused on key insights.";
        }

        private string BuildUserPrompt(GeneralEvaluationRequest request)
        {
            // Build a more compact summary instead of full JSON
            var summary = new StringBuilder();
            summary.AppendLine($"Performance evaluation for {request.EvaluationPeriod} ({request.TotalEmployees} employees):\n");

            // Group by department for better organization
            var byDepartment = request.Employees.GroupBy(e => e.Department ?? "No Department");
            
            foreach (var deptGroup in byDepartment)
            {
                summary.AppendLine($"\n{deptGroup.Key} ({deptGroup.Count()} employees):");
                
                foreach (var emp in deptGroup)
                {
                    var approvalRate = emp.Requests.Total > 0 
                        ? Math.Round((double)emp.Requests.Approved / emp.Requests.Total * 100, 1)
                        : 0.0;
                    
                    summary.AppendLine($"  - {emp.FullName} ({emp.Role} L{emp.Level}): " +
                        $"Attendance: {emp.Attendance.AttendanceRate:F1}% " +
                        $"(Present:{emp.Attendance.PresentDays} Late:{emp.Attendance.LateDays} Absent:{emp.Attendance.AbsentDays}), " +
                        $"Avg Hours: {emp.Attendance.AverageWorkingHours:F1}h, " +
                        $"Requests: {emp.Requests.Total} (Approved:{emp.Requests.Approved} ApprovalRate:{approvalRate}%)");
                }
            }

            return $@"Analyze the following employee performance data for the last 7 days:

{summary}

Provide a concise, well-formatted performance evaluation report. Consider:
- Attendance metrics: attendance rate, punctuality (on-time vs late), working hours consistency
- Request behavior: total requests, approval rate, types of requests (leave, resignation, overtime)
- Overall consistency and reliability

Evaluation criteria:
- Attendance rate above 90% = excellent, 80-90% = good, 70-80% = fair, below 70% = poor
- Late days: 0 = excellent, 1 = good, 2+ = needs improvement
- Request approval rate: above 80% = excellent, 60-80% = good, below 60% = needs improvement

Format your response as a clear, structured text report with proper headings and sections. Be professional, direct, and provide actionable insights. Keep it concise.";
        }

        public async Task<LevelPromotionEvaluationResponse> EvaluateLevelPromotionAsync(LevelPromotionEvaluationRequest request)
        {
            try
            {
                // Build prompt
                var systemMessage = BuildLevelPromotionSystemPrompt();
                var userMessage = BuildLevelPromotionUserPrompt(request);

                // Create request body with JSON response format
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = $"{systemMessage}\n\n{userMessage}"
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 3072, // Increased to allow for model's internal reasoning (thoughts)
                        responseMimeType = "application/json"
                    }
                };

                // Call Gemini API
                var response = await _httpClient.PostAsJsonAsync(_apiUrl, requestBody);
                
                // Handle rate limiting and other HTTP errors
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    throw new InvalidOperationException("Gemini API rate limit exceeded. Please try again in a few moments.");
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"Gemini API returned error: {response.StatusCode}. {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Log response for debugging (first 500 chars)
                Console.WriteLine($"Gemini API Response (first 500 chars): {responseContent.Substring(0, Math.Min(500, responseContent.Length))}");
                
                // Try to parse as standard Gemini response first
                var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                string? jsonText = null;

                // Check if response has candidates with parts
                if (geminiResponse?.Candidates != null && geminiResponse.Candidates.Length > 0)
                {
                    var candidate = geminiResponse.Candidates[0];
                    
                    // Check finish reason - MAX_TOKENS is acceptable if we can extract partial response
                    if (!string.IsNullOrEmpty(candidate.FinishReason) && candidate.FinishReason != "STOP" && candidate.FinishReason != "MAX_TOKENS")
                    {
                        var reason = candidate.FinishReason;
                        var safetyInfo = candidate.SafetyRatings != null 
                            ? string.Join(", ", candidate.SafetyRatings.Select(s => $"{s.Category}:{s.Probability}"))
                            : "unknown";
                        throw new InvalidOperationException($"Gemini API response blocked. FinishReason: {reason}, SafetyRatings: {safetyInfo}");
                    }
                    
                    // Check prompt feedback
                    if (geminiResponse.PromptFeedback?.BlockReason != null && geminiResponse.PromptFeedback.BlockReason.Length > 0)
                    {
                        var blockReasons = string.Join(", ", geminiResponse.PromptFeedback.BlockReason.Select(s => $"{s.Category}:{s.Probability}"));
                        throw new InvalidOperationException($"Gemini API prompt blocked. BlockReason: {blockReasons}");
                    }
                    
                    if (candidate.Content?.Parts != null && candidate.Content.Parts.Length > 0)
                    {
                        jsonText = candidate.Content.Parts[0].Text;
                    }
                    else if (candidate.FinishReason == "MAX_TOKENS")
                    {
                        // If MAX_TOKENS and no content, return no recommendation
                        Console.WriteLine("Warning: Response hit MAX_TOKENS limit with no content, returning no recommendation");
                        return new LevelPromotionEvaluationResponse
                        {
                            HasRecommendation = false,
                            RecommendedUserId = null,
                            RecommendedUserName = null,
                            CurrentLevel = null,
                            RecommendedLevel = null,
                            Reason = null
                        };
                    }
                }

                // If no text from parts, try parsing response directly as JSON (for JSON response format)
                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    // Try to extract JSON from response - might be direct JSON or wrapped
                    try
                    {
                        // Check if response is already a valid LevelPromotionEvaluationResponse
                        var directResponse = JsonSerializer.Deserialize<LevelPromotionEvaluationResponse>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (directResponse != null)
                        {
                            return directResponse;
                        }
                    }
                    catch
                    {
                        // Not direct JSON, continue
                    }

                    // Try to find JSON in the response content
                    // Sometimes Gemini wraps JSON in markdown code blocks or other text
                    var jsonStart = responseContent.IndexOf('{');
                    var jsonEnd = responseContent.LastIndexOf('}');
                    if (jsonStart >= 0 && jsonEnd > jsonStart)
                    {
                        jsonText = responseContent.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    }
                    else
                    {
                        // Log full response for debugging
                        Console.WriteLine($"Full Gemini API Response: {responseContent}");
                        throw new InvalidOperationException($"Invalid response from Gemini API: No content parts found. Response: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}");
                    }
                }

                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    Console.WriteLine($"Full Gemini API Response: {responseContent}");
                    throw new InvalidOperationException("Invalid response from Gemini API: Empty text");
                }

                // Parse JSON response
                var promotionResponse = JsonSerializer.Deserialize<LevelPromotionEvaluationResponse>(jsonText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (promotionResponse == null)
                {
                    Console.WriteLine($"Failed to parse JSON text: {jsonText}");
                    throw new InvalidOperationException("Failed to parse level promotion evaluation response");
                }

                return promotionResponse;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new InvalidOperationException("Gemini API request timed out. Please try again.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new InvalidOperationException("Gemini API request was cancelled.", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to call Gemini API: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse Gemini API response: {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                throw new InvalidOperationException("Gemini API request timed out. Please try again.", ex);
            }
        }

        private string BuildLevelPromotionSystemPrompt()
        {
            return @"You are a professional HR performance analyst. Analyze employee performance data for level promotion evaluation.

You must return ONLY a valid JSON object with the following structure:
{
  ""hasRecommendation"": boolean,
  ""recommendedUserId"": number or null,
  ""recommendedUserName"": string or null,
  ""currentLevel"": number or null,
  ""recommendedLevel"": number or null,
  ""reason"": string or null
}

Rules:
- If no candidate deserves promotion, set hasRecommendation to false and all other fields to null
- If one candidate deserves promotion, set hasRecommendation to true and provide their details
- Only recommend ONE candidate per evaluation
- recommendedLevel should be currentLevel + 1
- reason should be a brief explanation (2-3 sentences) in English
- Return ONLY the JSON object, no additional text or markdown formatting";
        }

        private string BuildLevelPromotionUserPrompt(LevelPromotionEvaluationRequest request)
        {
            // Pre-filter: Only consider candidates with attendance rate > 85% and late days <= 2 (stricter criteria)
            var eligibleCandidates = request.Candidates
                .Where(c => c.Attendance.AttendanceRate > 85 && c.Attendance.LateDays <= 2 && c.Attendance.AverageWorkingHours >= 7.0)
                .ToList();

            // Log for debugging
            Console.WriteLine($"Total candidates: {request.TotalCandidates}, Eligible after filter: {eligibleCandidates.Count}");
            if (eligibleCandidates.Any())
            {
                var top = eligibleCandidates.OrderByDescending(c => c.Attendance.AttendanceRate).First();
                Console.WriteLine($"Top candidate: {top.FullName}, AttRate: {top.Attendance.AttendanceRate}%, Late: {top.Attendance.LateDays}, AvgHrs: {top.Attendance.AverageWorkingHours}, ApprovalRate: {(top.Requests.Total > 0 ? (top.Requests.Approved / (double)top.Requests.Total * 100) : 0):F1}%");
            }

            // If no eligible candidates after strict filter, relax criteria slightly
            if (eligibleCandidates.Count == 0)
            {
                Console.WriteLine("No candidates passed strict filter, relaxing criteria...");
                eligibleCandidates = request.Candidates
                    .Where(c => c.Attendance.AttendanceRate > 80 && c.Attendance.LateDays <= 3 && c.Attendance.AverageWorkingHours >= 6.5)
                    .ToList();
                Console.WriteLine($"Relaxed filter eligible: {eligibleCandidates.Count}");
            }

            // Limit to top 10 candidates by attendance rate and avg hours to avoid prompt being too long
            var topCandidates = eligibleCandidates
                .OrderByDescending(c => c.Attendance.AttendanceRate)
                .ThenByDescending(c => c.Attendance.AverageWorkingHours)
                .ThenByDescending(c => c.Requests.Approved / (double)Math.Max(1, c.Requests.Total))
                .Take(10)
                .ToList();

            // If still no candidates after relaxed filter, take top candidates anyway for LLM to evaluate
            if (topCandidates.Count == 0)
            {
                Console.WriteLine("No candidates after relaxed filter, taking top 5 by attendance rate anyway...");
                topCandidates = request.Candidates
                    .OrderByDescending(c => c.Attendance.AttendanceRate)
                    .ThenByDescending(c => c.Attendance.AverageWorkingHours)
                    .Take(5)
                    .ToList();
                Console.WriteLine($"Sending {topCandidates.Count} candidates to LLM for evaluation");
            }

            var summary = new StringBuilder();
            summary.AppendLine($"Level promotion evaluation for {request.Role} role ({request.EvaluationPeriod}, {topCandidates.Count} top candidates from {request.TotalCandidates} total):\n");
            summary.AppendLine("Format: ID|Name|Level|Dept|AttRate%|Present|Late|Absent|AvgHrs|ReqTotal|ReqApproved|ReqApprovalRate%");
            summary.AppendLine();

            foreach (var candidate in topCandidates)
            {
                var approvalRate = candidate.Requests.Total > 0 
                    ? Math.Round((double)candidate.Requests.Approved / candidate.Requests.Total * 100, 1)
                    : 0.0;
                
                // Compact one-line format
                summary.AppendLine($"{candidate.Id}|{candidate.FullName}|L{candidate.Level}|{candidate.Department ?? "N/A"}|{candidate.Attendance.AttendanceRate:F1}%|{candidate.Attendance.PresentDays}|{candidate.Attendance.LateDays}|{candidate.Attendance.AbsentDays}|{candidate.Attendance.AverageWorkingHours:F1}h|{candidate.Requests.Total}|{candidate.Requests.Approved}|{approvalRate}%");
            }
            
            // Log what we're sending to LLM
            Console.WriteLine($"Sending {topCandidates.Count} candidates to LLM:");
            foreach (var c in topCandidates)
            {
                var apr = c.Requests.Total > 0 ? (c.Requests.Approved / (double)c.Requests.Total * 100) : 0;
                Console.WriteLine($"  - {c.FullName}: AttRate={c.Attendance.AttendanceRate:F1}%, Late={c.Attendance.LateDays}, AvgHrs={c.Attendance.AverageWorkingHours:F1}h, ApprovalRate={apr:F1}%");
            }

            return $@"Evaluate for promotion. Format: ID|Name|Level|Dept|AttRate%|Present|Late|Absent|AvgHrs|ReqTotal|ReqApproved|ReqApprovalRate%

{summary}

Evaluation criteria (all must be met for promotion): Attendance rate >85%, Late days ≤2, Average hours ≥7h, Approval rate >80%. 
If a candidate clearly exceeds these standards, recommend them. Otherwise return hasRecommendation:false with reason explaining why.
Return JSON: {{""hasRecommendation"":bool,""recommendedUserId"":int|null,""recommendedUserName"":str|null,""currentLevel"":int|null,""recommendedLevel"":int|null,""reason"":str|null}}.";
        }

        // Helper classes for Gemini API response
        private class GeminiApiResponse
        {
            public Candidate[]? Candidates { get; set; }
            public PromptFeedback? PromptFeedback { get; set; }
        }

        private class Candidate
        {
            public Content? Content { get; set; }
            public string? FinishReason { get; set; }
            public SafetyRating[]? SafetyRatings { get; set; }
        }

        private class Content
        {
            public Part[]? Parts { get; set; }
        }

        private class Part
        {
            public string? Text { get; set; }
        }

        private class PromptFeedback
        {
            public SafetyRating[]? BlockReason { get; set; }
        }

        private class SafetyRating
        {
            public string? Category { get; set; }
            public string? Probability { get; set; }
        }
    }
}

