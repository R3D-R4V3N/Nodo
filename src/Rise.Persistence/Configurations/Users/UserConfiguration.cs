using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Users;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;


namespace Rise.Persistence.Configurations.Users;

internal class UserConfiguration : EntityConfiguration<ApplicationUser>
{
    public override void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.AccountId).IsRequired().HasMaxLength(36);
        builder.HasIndex(x => x.AccountId).IsUnique();

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Biography).IsRequired().HasMaxLength(500);
        builder.Property(x => x.AvatarUrl).IsRequired().HasMaxLength(250);
        builder.Property(x => x.BirthDay).IsRequired();
        builder.Property(x => x.UserType).IsRequired();

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
            .WithOne()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        // connections
        builder.Ignore(u => u.Connections);
        builder.Ignore(u => u.Friends);
        builder.Ignore(u => u.FriendRequests);
        builder.Ignore(u => u.BlockedUsers);

        builder.OwnsMany<UserConnection>("_connections", connections =>
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
                .HasDefaultValue(12);

            userSettings.OwnsMany(s => s.ChatTextLineSuggestions, nav =>
            {
                nav.ToTable("UserSettingChatTextLineSuggestions");
                nav.WithOwner()
                    .HasForeignKey("UserSettingsId");

                nav.Property(p => p.Text)
                   .HasColumnName("TextSuggestion")
                   .IsRequired();

                nav.Property(p => p.Rank)
                   .IsRequired();
            });

            userSettings.ToTable("UserSetting");
        });
}

}
