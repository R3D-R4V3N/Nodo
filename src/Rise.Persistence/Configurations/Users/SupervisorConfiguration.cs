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

        builder.HasOne(supervisor => supervisor.Organization)
            .WithMany(organization => organization.Supervisors)
            .HasForeignKey("OrganizationId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
