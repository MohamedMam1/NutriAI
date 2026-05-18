using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutriAI.Domain.Entities;

namespace NutriAI.Infrastructure.Data.Configurations;

public class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
{
    public void Configure(EntityTypeBuilder<MealPlan> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.DietaryPreference).HasMaxLength(100);
        builder.HasMany(x => x.Items)
            .WithOne(x => x.MealPlan)
            .HasForeignKey(x => x.MealPlanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User)
            .WithMany(x => x.MealPlans)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
