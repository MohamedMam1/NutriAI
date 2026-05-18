using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutriAI.Domain.Entities;

namespace NutriAI.Infrastructure.Data.Configurations;

public class UserGoalConfiguration : IEntityTypeConfiguration<UserGoal>
{
    public void Configure(EntityTypeBuilder<UserGoal> builder)
    {
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.Property(x => x.Gender).HasMaxLength(20);
        builder.Property(x => x.ActivityLevel).HasMaxLength(50);
        builder.HasOne(x => x.User)
            .WithOne(x => x.UserGoal)
            .HasForeignKey<UserGoal>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
