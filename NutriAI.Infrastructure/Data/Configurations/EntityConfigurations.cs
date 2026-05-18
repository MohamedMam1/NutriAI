using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutriAI.Domain.Entities;

namespace NutriAI.Infrastructure.Data.Configurations;

public class WeightLogConfiguration : IEntityTypeConfiguration<WeightLog>
{
    public void Configure(EntityTypeBuilder<WeightLog> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.LoggedAt });
        builder.HasOne(x => x.User).WithMany(x => x.WeightLogs).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WaterLogConfiguration : IEntityTypeConfiguration<WaterLog>
{
    public void Configure(EntityTypeBuilder<WaterLog> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.LoggedAt });
        builder.HasOne(x => x.User).WithMany(x => x.WaterLogs).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WeeklyReportConfiguration : IEntityTypeConfiguration<WeeklyReport>
{
    public void Configure(EntityTypeBuilder<WeeklyReport> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.WeekStart });
        builder.HasOne(x => x.User).WithMany(x => x.WeeklyReports).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AIChatConfiguration : IEntityTypeConfiguration<AIChat>
{
    public void Configure(EntityTypeBuilder<AIChat> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.Context, x.CreatedAt });
        builder.HasOne(x => x.User).WithMany(x => x.AIChats).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.IsRead });
        builder.HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class MealConfiguration : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> builder)
    {
        builder.HasOne(x => x.User).WithMany(x => x.Meals).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
