using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Users.Hobbys;

namespace Rise.Persistence.Configurations.Users;

internal sealed class UserHobbyConfiguration : IEntityTypeConfiguration<UserHobby>
{
    public void Configure(EntityTypeBuilder<UserHobby> builder)
    {
        builder.ToTable("UserHobbies");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .ValueGeneratedOnAdd();

        builder.Property<int>("UserId");
        builder.HasIndex("UserId");

        builder.Property(h => h.Hobby)
            .HasConversion(
                hobby => hobby.ToString(),
                value => ConvertLegacyHobby(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Ignore(h => h.CreatedAt);
        builder.Ignore(h => h.UpdatedAt);
        builder.Ignore(h => h.IsDeleted);
    }

    private static HobbyType ConvertLegacyHobby(string? value)
    {
        var normalized = value ?? string.Empty;

        if (Enum.TryParse<HobbyType>(normalized, out var hobby))
        {
            return hobby;
        }

        return normalized switch
        {
            "Music" => HobbyType.MusicMaking,
            "Crafts" => HobbyType.Crafting,
            "Cards" => HobbyType.CardGames,
            "Travel" => HobbyType.Hiking,
            "Movies" => HobbyType.Photography,
            "Series" => HobbyType.BoardGames,
            "Animals" => HobbyType.Birdwatching,
            "Fitness" => HobbyType.Running,
            _ => HobbyType.Crafting
        };
    }
}
