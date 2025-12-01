using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Users;


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

        builder.OwnsOne(m => m.FirstName, firstname =>
        {
            firstname.Property(t => t.Value)
                .HasColumnName("FirstName")
                .HasMaxLength(FirstName.MAX_LENGTH);
        });

        builder.OwnsOne(m => m.LastName, lastName =>
        {
            lastName.Property(t => t.Value)
                .HasColumnName("LastName")
                .HasMaxLength(LastName.MAX_LENGTH);
        });

        builder.OwnsOne(m => m.Biography, bio =>
        {
            bio.Property(t => t.Value)
                .HasColumnName("Biography")
                .HasMaxLength(Biography.MAX_LENGTH);
        });

        builder.OwnsOne(m => m.AvatarUrl, bio =>
        {
            bio.Property(t => t.Value)
                .HasColumnName("AvatarUrl")
                .HasMaxLength(AvatarUrl.MAX_LENGTH);
        });

        builder.OwnsOne(m => m.BirthDay, bd =>
        {
            bd.Property(t => t.Value)
                .HasColumnName("BirthDay");
        });

        builder.Property(x => x.Gender)
            .HasDefaultValue(GenderType.X);


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

            userSettings.OwnsOne(m => m.FontSize, fontSize =>
            {
                fontSize.Property(t => t.Value)
                    .HasColumnName("FontSize")
                    .HasDefaultValue(12);
            });

            userSettings.OwnsMany(s => s.ChatTextLineSuggestions, nav =>
            {
                nav.ToTable("UserSettingChatTextLineSuggestions");
                nav.WithOwner()
                    .HasForeignKey("UserSettingsId");

                nav.OwnsOne(m => m.Sentence, sentence =>
                {
                    sentence.Property(t => t.Value)
                        .IsRequired()
                        .HasMaxLength(DefaultSentence.MAX_LENGTH)
                        .HasColumnName("TextSuggestion");
                });

                nav.Property(p => p.Rank)
                   .IsRequired();
            });

            userSettings.ToTable("UserSetting");
        });
    }
}
