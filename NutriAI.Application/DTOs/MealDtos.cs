namespace NutriAI.Application.DTOs;

public record MealAnalyzeRequestDto(string Description);
public record MealLogDto(int Id, string Description, int Calories, double Protein, double Carbs, double Fat, string Time, string? AiResponse);
public record MealAnalyzeResponseDto(bool Success, string Message, MealLogDto Meal, string AiResponse);
