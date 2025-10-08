using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Message;
using Rise.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Persistence.Configurations.Users
{
    internal class ApplicationUserSettingsConfiguration : EntityConfiguration<ApplicationUserSettings>
    {
        public override void Configure(EntityTypeBuilder<ApplicationUserSettings> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.IsDarkMode)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.NotificationsEnabled)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.FontSize)
                .HasDefaultValue(12)
                .IsRequired();

            builder.Property(x => x.CommonPhrases)
                .HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasColumnName("CommonPhrases");
        }
    }
}
