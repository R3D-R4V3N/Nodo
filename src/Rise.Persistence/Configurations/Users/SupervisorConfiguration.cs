using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Users;


namespace Rise.Persistence.Configurations.Users;

internal class SupervisorConfiguration : EntityConfiguration<Supervisor>
{
    public override void Configure(EntityTypeBuilder<Supervisor> builder)
    {
        base.Configure(builder);

        builder.ToTable("Supervisors");

        // organisation
        builder.HasOne(x => x.Organization)
            .WithMany(x => x.Supervisors)
            .HasForeignKey("OrganizationId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
