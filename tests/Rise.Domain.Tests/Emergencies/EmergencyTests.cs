using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Emergencies;
using Rise.Domain.Users;
using Rise.Tests.Shared;
using Shouldly;

namespace Rise.Domain.Tests.Emergencies;
public class EmergencyTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        var chat = DomainData.ValidChat();
        var chatUser = chat.Users.First();
        var message = chat.AddTextMessage("test", chatUser).Value;

        var emergency = new Emergency
        {
            HappenedInChat = chat,
            MadeByUser = chatUser,
            Range = EmergencyRange.Create(DateTime.UtcNow),
            Type = EmergencyType.Other
        };

        emergency.HappenedInChat.ShouldBe(chat);
        emergency.MadeByUser.ShouldBe(chatUser);
        emergency.Type.ShouldBe(EmergencyType.Other);
        emergency.AllowedToResolve.Count.ShouldBe(2);
        emergency.HasResolved.ShouldBeEmpty();
        emergency.IsResolved.ShouldBeFalse();

        chat.Emergencies.ShouldContain(emergency);
    }

    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly_UsersShareSameSupervisor()
    {
        var chat = DomainData.ValidChat();
        var supervisor = DomainData.ValidSupervisor();

        foreach (User user in chat.Users)
        {
            user.Supervisor = supervisor;
        }
        var chatUser = chat.Users.First();
        var message = chat.AddTextMessage("test", chatUser).Value;

        var emergency = new Emergency
        {
            HappenedInChat = chat,
            MadeByUser = chatUser,
            Range = EmergencyRange.Create(DateTime.UtcNow),
            Type = EmergencyType.Other
        };

        emergency.HappenedInChat.ShouldBe(chat);
        emergency.MadeByUser.ShouldBe(chatUser);
        emergency.Type.ShouldBe(EmergencyType.Other);
        emergency.AllowedToResolve.Count.ShouldBe(1);
        emergency.HasResolved.ShouldBeEmpty();
        emergency.IsResolved.ShouldBeFalse();

        chat.Emergencies.ShouldContain(emergency);
    }

    [Fact]
    public void SetChat_ShouldSucceed_WhenSame()
    {
        var chat = DomainData.ValidChat();
        var chatUser = chat.Users.First();
        var message = chat.AddTextMessage("test", chatUser).Value;

        var emergency = new Emergency
        {
            HappenedInChat = chat,
            MadeByUser = chatUser,
            Range = EmergencyRange.Create(DateTime.UtcNow),
            Type = EmergencyType.Other
        };

        emergency.HappenedInChat = chat;

        chat.Emergencies.Count.ShouldBe(1);
        chat.Emergencies.ShouldContain(emergency);
        
        emergency.HappenedInChat.ShouldBe(chat);
        emergency.AllowedToResolve.Count.ShouldBe(2);
        emergency.HasResolved.ShouldBeEmpty();

    }

    [Fact]
    public void SetChat_ShouldThrow_WhenReassigning()
    {
        var chat = DomainData
            .ValidChat()
            .WithId(1);
        var chatUser = chat.Users.First();
        var message = chat.AddTextMessage("test", chatUser).Value;

        var emergency = new Emergency
        {
            HappenedInChat = chat,
            MadeByUser = chatUser,
            Range = EmergencyRange.Create(DateTime.UtcNow),
            Type = EmergencyType.Other
        };

        var chat2 = DomainData
            .ValidChat()
            .WithId(2);

        Action reassignChat = () => emergency.HappenedInChat = chat2;
        reassignChat.ShouldThrow<InvalidOperationException>();

        emergency.HappenedInChat.ShouldBe(chat);
        chat.Emergencies.Count.ShouldBe(1);
        chat2.Emergencies.Count.ShouldBe(0);
    }

    [Fact]
    public void Resolve_ShouldSucceed_NotResolvedAfter1partyLooksAtIt()
    {
        var emergency = DomainData.ValidEmergency();
        var chatUser = emergency.HappenedInChat.Users.First();

        var supervisor = ((User)chatUser).Supervisor;
        emergency.Resolve(supervisor);

        emergency.HasResolved.Count.ShouldBe(1);
        emergency.IsResolved.ShouldBeFalse();
    }

    [Fact]
    public void Resolve_ShouldSucceed_Resolved()
    {
        var emergency = DomainData.ValidEmergency();

        foreach (User user in emergency.HappenedInChat.Users)
        {
            emergency.Resolve(user.Supervisor);
        }

        emergency.HasResolved.Count.ShouldBe(2);
        emergency.IsResolved.ShouldBeTrue();
    }
}
