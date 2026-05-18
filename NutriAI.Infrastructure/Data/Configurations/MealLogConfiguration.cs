using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutriAI.Domain.Entities;

namespace NutriAI.Infrastructure.Data.Configurations;

public class MealLogConfiguration : IEntityTypeConfiguration<MealLog>
{
    public void Configure(EntityTypeBuilder<MealLog> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.LoggedAt });
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasOne(x => x.User)
            .WithMany(x => x.MealLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Meal)
            .WithMany(x => x.MealLogs)
            .HasForeignKey(x => x.MealId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
