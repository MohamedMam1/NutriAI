using System.ComponentModel.DataAnnotations;

namespace NutriAI.Application.DTOs;

public record MealAnalyzeRequestDto(
    [Required, MinLength(2), MaxLength(2000)] string Description);
public record MealLogDto(int Id, string Description, int Calories, double Protein, double Carbs, double Fat, string Time, string? AiResponse);
