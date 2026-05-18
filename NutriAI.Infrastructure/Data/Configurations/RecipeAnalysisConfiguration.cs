using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutriAI.Domain.Entities;

namespace NutriAI.Infrastructure.Data.Configurations;

public class RecipeAnalysisConfiguration : IEntityTypeConfiguration<RecipeAnalysis>
{
    public void Configure(EntityTypeBuilder<RecipeAnalysis> builder)
    {
        builder.HasOne(x => x.User)
            .WithMany(x => x.RecipeAnalyses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Recipe)
            .WithMany(x => x.Analyses)
            .HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
