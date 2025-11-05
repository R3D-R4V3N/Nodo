using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Common;
using Rise.Domain.Users.Sentiment;

namespace Rise.Persistence.Configurations.Users.Sentiments;

internal class SentimentConfiguration : EntityConfiguration<UserSentiment>
{
    public override void Configure(EntityTypeBuilder<UserSentiment> builder)
    {
        base.Configure(builder);

        builder.Property(s => s.Category)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(s => new { s.Type, s.Category })
            .IsUnique();

        builder.ToTable("Sentiments");
    }
}