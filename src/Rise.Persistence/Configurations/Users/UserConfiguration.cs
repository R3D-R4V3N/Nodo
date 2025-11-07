using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rise.Domain.Users;
using Rise.Persistence.Configurations.Users.Hobbies;
using Rise.Persistence.Configurations.Users.Sentiments;


namespace Rise.Persistence.Configurations.Users;

internal class UserConfiguration : EntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        builder.ToTable("Users");


        // connections
        builder.Ignore(u => u.Friends);
        builder.Ignore(u => u.FriendRequests);
        builder.Ignore(u => u.BlockedUsers);

        builder.HasMany(u => u.Connections)
            .WithOne(c => c.From)
            .HasForeignKey("FromId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Supervisor)
            .WithMany(s => s.AssignedUsers)
            .HasForeignKey(u => u.SupervisorId)
            .OnDelete(DeleteBehavior.SetNull);

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
    }
}
