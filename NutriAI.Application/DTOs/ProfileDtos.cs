namespace NutriAI.Application.DTOs;

public record ProfileDto(
    string Email,
    string Name,
    int Age,
    string Gender,
    double Height,
    double CurrentWeight,
    double GoalWeight,
    string ActivityLevel,
    int DailyWaterTargetMl);
