using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Common;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Settings;
using Rise.Domain.Users.Settings.Properties;


namespace Rise.Persistence.Configurations.Users;

internal class UserConfiguration : EntityConfiguration<ApplicationUser>
{
    public override void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.AccountId).IsRequired().HasMaxLength(36);
        builder.HasIndex(x => x.AccountId).IsUnique();

        builder.Property(x => x.FirstName)
            .HasConversion(
                new ValueObjectConverter<FirstName, string>()
            ).IsRequired()
            .HasMaxLength(FirstName.MAX_LENGTH);

        builder.Property(x => x.LastName)
            .HasConversion(
                new ValueObjectConverter<LastName, string>()
            ).IsRequired()
            .HasMaxLength(LastName.MAX_LENGTH);

        builder.Property(x => x.Biography)
            .HasConversion(
                new ValueObjectConverter<Biography, string>()
            ).IsRequired()
            .HasMaxLength(Biography.MAX_LENGTH);

        builder.Property(x => x.AvatarUrl)
            .HasConversion(
                new ValueObjectConverter<AvatarUrl, string>()
            ).IsRequired()
            .HasMaxLength(AvatarUrl.MAX_LENGTH);

        builder.Property(x => x.BirthDay).IsRequired();
        builder.Property(x => x.UserType).IsRequired();
        builder.Property(x => x.Gender).IsRequired().HasMaxLength(10);

        // sentiments
        builder.Ignore(u => u.Likes);
        builder.Ignore(u => u.Dislikes);
        builder.HasMany(u => u.Sentiments)
               .WithMany()
               .UsingEntity<UserSentimentJoin>(
                   j => j
                       .HasOne(js => js.Sentiment)
                       .WithMany()
                       .HasForeignKey(js => js.SentimentId),
                   j => j
                       .HasOne(js => js.User)
                       .WithMany()
                       .HasForeignKey(js => js.UserId),
                   j =>
                   {
                       j.ToTable("UserSentiments");
                       j.HasKey(x => new { x.UserId, x.SentimentId });
                   });

        // hobbies
        builder.HasMany(u => u.Hobbies)
               .WithMany()
               .UsingEntity<UserHobbyJoin>(
                   j => j
                       .HasOne(js => js.Hobby)
                       .WithMany()
                       .HasForeignKey(js => js.HobbyId),
                   j => j
                       .HasOne(js => js.User)
                       .WithMany()
                       .HasForeignKey(js => js.UserId),
                   j =>
                   {
                       j.ToTable("UserHobbies");
                       j.HasKey(x => new { x.UserId, x.HobbyId });
                   });

        builder.Navigation(u => u.Hobbies)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // connections
        builder.Ignore(u => u.Friends);
        builder.Ignore(u => u.FriendRequests);
        builder.Ignore(u => u.BlockedUsers);

        builder.OwnsMany(u => u.Connections, connections =>
        {
            connections.WithOwner()
                        .HasForeignKey("UserId");

            // shadow key
            connections.Property<int>("Id");
            connections.HasKey("Id");

            connections.Property(c => c.ConnectionType)
                        .HasConversion<string>()
                        .IsRequired();

            connections.Property(c => c.CreatedAt)
                .IsRequired();

            connections.Property<int>("UserConnectionId")
                        .IsRequired();

            connections.HasOne(c => c.Connection)
                        .WithMany()
                        .HasForeignKey("UserConnectionId")
                        .OnDelete(DeleteBehavior.Cascade);

            connections.ToTable("UserConnections");
        });

        // settings
        builder.Ignore(u => u.UserSettings);

        builder.OwnsOne<ApplicationUserSetting>("_userSettings", userSettings => 
        {
            userSettings.WithOwner(s => s.User)
                .HasForeignKey("UserId");

            //shadow key
            userSettings.Property<int>("Id");
            userSettings.HasKey("Id");

            userSettings.Property(s=> s.IsDarkMode)
                .HasDefaultValue(false);

            userSettings.Property(s => s.FontSize)
                .HasConversion(
                    new ValueObjectConverter<FontSize, int>()
                )
                .HasDefaultValue(FontSize.Create(12).Value);

            userSettings.OwnsMany(s => s.ChatTextLineSuggestions, nav =>
            {
                nav.ToTable("UserSettingChatTextLineSuggestions");
                nav.WithOwner()
                    .HasForeignKey("UserSettingsId");

                nav.Property(p => p.Sentence)
                    .HasConversion(
                        new ValueObjectConverter<DefaultSentence, string>()
                    ).IsRequired()
                    .HasMaxLength(DefaultSentence.MAX_LENGTH)
                    .HasColumnName("TextSuggestion");

                nav.Property(p => p.Rank)
                   .IsRequired();
            });

            userSettings.ToTable("UserSetting");
        });
}

}
