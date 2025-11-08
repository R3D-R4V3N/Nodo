using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Organizations;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Settings;
using Rise.Domain.Users.Settings.Properties;


namespace Rise.Persistence.Configurations.Users;

internal class BaseUserConfiguration : EntityConfiguration<BaseUser>
{
    public override void Configure(EntityTypeBuilder<BaseUser> builder)
    {
        base.Configure(builder);

        builder.UseTptMappingStrategy();
        builder.ToTable("BaseUsers");

        builder.Property(x => x.AccountId).IsRequired().HasMaxLength(36);
        builder.HasIndex(x => x.AccountId).IsUnique();

        builder.Property(x => x.FirstName)
            .HasConversion(
                new PropertyConverter<FirstName, string>()
            ).IsRequired()
            .HasMaxLength(FirstName.MAX_LENGTH);

        builder.Property(x => x.LastName)
            .HasConversion(
                new PropertyConverter<LastName, string>()
            ).IsRequired()
            .HasMaxLength(LastName.MAX_LENGTH);

        builder.Property(x => x.Biography)
            .HasConversion(
                new PropertyConverter<Biography, string>()
            ).IsRequired()
            .HasMaxLength(Biography.MAX_LENGTH);

        builder.Property(x => x.AvatarUrl)
            .HasConversion(
                new PropertyConverter<AvatarUrl, string>()
            ).IsRequired()
            .HasColumnType("longtext");

        builder.Property(x => x.BirthDay).IsRequired();
        builder.Property(x => x.Gender)
            .HasDefaultValue(GenderType.X);

        builder.HasOne(u => u.Organization)
            .WithMany(o => o.Members)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // settings
        builder.OwnsOne(u => u.UserSettings, userSettings =>
        {
            userSettings.WithOwner(s => s.User)
                .HasForeignKey("UserId");

            //shadow key
            userSettings.Property<int>("Id");
            userSettings.HasKey("Id");

            userSettings.Property(s => s.IsDarkMode)
                .HasDefaultValue(false);

            userSettings.Property(s => s.FontSize)
                .HasConversion(
                    new PropertyConverter<FontSize, int>()
                )
                .HasDefaultValue(FontSize.Create(12).Value);

            userSettings.OwnsMany(s => s.ChatTextLineSuggestions, nav =>
            {
                nav.ToTable("UserSettingChatTextLineSuggestions");
                nav.WithOwner()
                    .HasForeignKey("UserSettingsId");

                nav.Property(p => p.Sentence)
                    .HasConversion(
                        new PropertyConverter<DefaultSentence, string>()
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
