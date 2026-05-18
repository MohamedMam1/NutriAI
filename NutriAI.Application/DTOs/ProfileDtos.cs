namespace NutriAI.Application.DTOs;

public record ProfileDto(
    string Name,
    int Age,
    string Gender,
    double Height,
    double CurrentWeight,
    double GoalWeight,
    string ActivityLevel);
