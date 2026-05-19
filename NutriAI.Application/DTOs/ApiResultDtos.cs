namespace NutriAI.Application.DTOs;

public record ApiResultDto<T>(
    bool Success,
    T? Data,
    string DataSource,
    string Message);

public record MealAnalyzeResponseDto(
    bool Success,
    string Message,
    MealLogDto? Meal,
    string? AiResponse,
    string DataSource);
