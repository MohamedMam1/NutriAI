using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NutriAI.Domain.Entities;

namespace NutriAI.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserGoal> UserGoals => Set<UserGoal>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<MealLog> MealLogs => Set<MealLog>();
    public DbSet<WeightLog> WeightLogs => Set<WeightLog>();
    public DbSet<WaterLog> WaterLogs => Set<WaterLog>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<MealPlanItem> MealPlanItems => Set<MealPlanItem>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeAnalysis> RecipeAnalyses => Set<RecipeAnalysis>();
    public DbSet<WeeklyReport> WeeklyReports => Set<WeeklyReport>();
    public DbSet<AIChat> AIChats => Set<AIChat>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SeedMealTemplate> SeedMealTemplates => Set<SeedMealTemplate>();
    public DbSet<SeedMealPlanTemplate> SeedMealPlanTemplates => Set<SeedMealPlanTemplate>();
    public DbSet<SeedRecipeTemplate> SeedRecipeTemplates => Set<SeedRecipeTemplate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
