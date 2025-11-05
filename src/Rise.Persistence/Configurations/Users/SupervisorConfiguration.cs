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
    }
}
