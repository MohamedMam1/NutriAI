using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NutriAI.Application.Configuration;
using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Services;

namespace NutriAI.Infrastructure.AI;

public class OpenAiNutritionService : IAiNutritionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _httpClient;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<OpenAiNutritionService> _logger;

    public OpenAiNutritionService(
        HttpClient httpClient,
        IOptions<OpenAiSettings> settings,
        ILogger<OpenAiNutritionService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.ApiKey);

    public Task<MealAnalysisResult?> AnalyzeMealAsync(string description, UserNutritionContext context, CancellationToken cancellationToken = default) =>
        SendJsonAsync<MealAnalysisResult>(
            """
            You are a nutrition assistant. Estimate calories and macros for the meal described.
            Respond ONLY with valid JSON:
            {"calories":number,"protein":number,"carbs":number,"fat":number,"aiResponse":"string explaining impact on daily calorie goal and weight progress"}
            """,
            $"Meal: {description}\nDaily calorie target: {context.DailyCalorieTarget}. Current weight: {context.CurrentWeightKg}kg, goal: {context.GoalWeightKg}kg.",
            cancellationToken);

    public async Task<IReadOnlyList<MealPlanDayResult>?> GenerateMealPlanAsync(
        double goalWeightKg,
        int timelineWeeks,
        string dietaryPreference,
        UserNutritionContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await SendJsonAsync<MealPlanResponse>(
            """
            You are a meal planning nutritionist. Create a 7-day plan (Monday-Sunday) with breakfast, lunch, dinner, snacks.
            Each meal needs low-calorie preparation instructions.
            Respond ONLY with valid JSON:
            {"days":[{"day":"Monday","meals":[{"mealType":"Breakfast","name":"string","calories":number,"protein":number,"carbs":number,"fat":number,"instructions":"string"}]}]}
            """,
            $"Goal weight: {goalWeightKg}kg in {timelineWeeks} weeks. Preference: {dietaryPreference}. Daily calories ~{context.DailyCalorieTarget}. Activity: {context.ActivityLevel}.",
            cancellationToken);

        return result?.Days;
    }

    public Task<RecipeAnalysisResult?> AnalyzeRecipeAsync(string recipeText, CancellationToken cancellationToken = default) =>
        SendJsonAsync<RecipeAnalysisResult>(
            """
            Parse the recipe and estimate nutrition per ingredient and totals.
            Respond ONLY with valid JSON:
            {"recipeName":"string","totalCalories":number,"servings":number,"protein":number,"carbs":number,"fat":number,
            "ingredients":[{"name":"string","amount":"string","calories":number}],
            "alternatives":["string"]}
            """,
            recipeText,
            cancellationToken);

    public async Task<IReadOnlyList<string>?> GetWeeklyRecommendationsAsync(string reportSummary, CancellationToken cancellationToken = default)
    {
        var result = await SendJsonAsync<TipsResponse>(
            "Provide exactly 3 actionable weekly nutrition tips. Respond ONLY with JSON: {\"tips\":[\"tip1\",\"tip2\",\"tip3\"]}",
            reportSummary,
            cancellationToken);

        return result?.Tips;
    }

    public Task<string?> GetHydrationRecommendationAsync(
        UserNutritionContext context,
        int currentMl,
        int todayCalories,
        CancellationToken cancellationToken = default) =>
        SendTextAsync(
            "Give one concise hydration recommendation (max 2 sentences). No markdown.",
            $"Water today: {currentMl}ml of {context.DailyWaterTargetMl}ml goal. Calories today: {todayCalories}/{context.DailyCalorieTarget}. Activity: {context.ActivityLevel}. Weight goal: {context.GoalWeightKg}kg.",
            cancellationToken);

    public Task<string?> GetDashboardInsightAsync(
        UserNutritionContext context,
        int caloriesConsumed,
        int waterMl,
        CancellationToken cancellationToken = default) =>
        SendTextAsync(
            "Give one encouraging dashboard nutrition insight (max 2 sentences). No markdown.",
            $"Calories: {caloriesConsumed}/{context.DailyCalorieTarget}. Water: {waterMl}/{context.DailyWaterTargetMl}ml. Weight: {context.CurrentWeightKg}kg -> {context.GoalWeightKg}kg.",
            cancellationToken);

    public Task<string?> GetWeightInsightAsync(
        UserNutritionContext context,
        double latestWeight,
        CancellationToken cancellationToken = default) =>
        SendTextAsync(
            "Explain briefly how consistent logging supports reaching the weight goal (max 2 sentences). No markdown.",
            $"Latest weight: {latestWeight}kg. Goal: {context.GoalWeightKg}kg. Calorie target: {context.DailyCalorieTarget}.",
            cancellationToken);

    private async Task<T?> SendJsonAsync<T>(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        var content = await SendChatAsync(systemPrompt, userPrompt, jsonMode: true, cancellationToken);
        if (string.IsNullOrWhiteSpace(content)) return default;

        try
        {
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse OpenAI JSON response");
            return default;
        }
    }

    private async Task<string?> SendTextAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken) =>
        await SendChatAsync(systemPrompt, userPrompt, jsonMode: false, cancellationToken);

    private async Task<string?> SendChatAsync(string systemPrompt, string userPrompt, bool jsonMode, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            _logger.LogDebug("OpenAI API key not configured; skipping AI call.");
            return null;
        }

        try
        {
            var body = new Dictionary<string, object>
            {
                ["model"] = _settings.Model,
                ["temperature"] = 0.4,
                ["messages"] = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            if (jsonMode)
                body["response_format"] = new { type = "json_object" };

            var requestUri = $"{_settings.BaseUrl.TrimEnd('/')}/chat/completions";
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI API error {Status}: {Body}", response.StatusCode, responseJson);
                return null;
            }

            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "OpenAI request failed");
            return null;
        }
    }

    private sealed class MealPlanResponse
    {
        public List<MealPlanDayResult>? Days { get; set; }
    }

    private sealed class TipsResponse
    {
        public List<string>? Tips { get; set; }
    }
}
