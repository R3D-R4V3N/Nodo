using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Users;


namespace Rise.Persistence.Configurations.Users;

internal class UserConfiguration : EntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        builder.ToTable("Users");

        // organisation
        builder.HasOne(user => user.Organization)
            .WithMany(organization => organization.Users)
            .HasForeignKey("OrganizationId")
            .OnDelete(DeleteBehavior.Restrict);

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
    }
}
