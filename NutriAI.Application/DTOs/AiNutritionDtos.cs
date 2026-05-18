namespace NutriAI.Application.DTOs;

public record UserNutritionContext(
    int DailyCalorieTarget,
    double CurrentWeightKg,
    double GoalWeightKg,
    int DailyWaterTargetMl,
    string ActivityLevel,
    int Age,
    string Gender);

public record MealAnalysisResult(
    int Calories,
    double Protein,
    double Carbs,
    double Fat,
    string AiResponse);

public record MealPlanDayItem(
    string MealType,
    string Name,
    int Calories,
    double Protein,
    double Carbs,
    double Fat,
    string Instructions);

public record MealPlanDayResult(string Day, IReadOnlyList<MealPlanDayItem> Meals);

public record RecipeIngredientResult(string Name, string Amount, int Calories);

public record RecipeAnalysisResult(
    string RecipeName,
    int TotalCalories,
    int Servings,
    double Protein,
    double Carbs,
    double Fat,
    IReadOnlyList<RecipeIngredientResult> Ingredients,
    IReadOnlyList<string> Alternatives);
