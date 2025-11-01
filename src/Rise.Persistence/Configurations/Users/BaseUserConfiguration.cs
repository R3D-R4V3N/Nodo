using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Organizations;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Settings;
using Rise.Domain.Users.Settings.Properties;
using System.Reflection.Emit;


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

        builder.HasOne(x => x.Organization)
            .WithMany(organization => organization.Members)
            .HasForeignKey("OrganizationId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property<int>("OrganizationId")
            .IsRequired();

        // settings
        builder.Ignore(u => u.UserSettings);

        builder.OwnsOne<UserSetting>("_userSettings", userSettings => 
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
