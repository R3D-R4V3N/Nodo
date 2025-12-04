using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Tests.Fakers;
using Rise.Services.Tests.Helpers;
using Rise.Services.UserConnections;
using Rise.Shared.UserConnections;
using Rise.Tests.Shared;
using System.Transactions;

namespace Rise.Services.Tests.UserConnections;
public class AcceptFriendRequestAsyncTests_Refriend : IClassFixture<EFFixture>
{
    private EFFixture _fixture;

    public AcceptFriendRequestAsyncTests_Refriend(EFFixture fixture)
    {
        _fixture = fixture;
    }

    private (IUserConnectionService Service, FakeUserConnectionNotificationDispatcher NotificationDispatcher) 
        CreateUCService(User? loggedInUser, ApplicationDbContext dbcontext)
    {
        if (_fixture is null)
            throw new ArgumentNullException(nameof(_fixture));

        var notificationDispatcher = new FakeUserConnectionNotificationDispatcher();

        return (
            new UserConnectionService(
                dbcontext,
                new FakeSessionContextProvider(ServicesData.GetValidClaimsPrincipal(loggedInUser)),
                notificationDispatcher
            ),
            notificationDispatcher
        );
    }

    [Fact]
    public async Task AcceptFriendRequestAsync_ShouldAccept_WhenRequestExists()
    {
        var db = _fixture.CreateApplicationDbContext();

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        alice.SendFriendRequest(bob);
        bob.AcceptFriendRequest(alice);
        var chat = Chat.CreatePrivateChat(alice, bob);
        alice.RemoveFriend(bob);
        bob.SendFriendRequest(alice);

        db.Users.AddRange(alice, bob);
        db.Chats.Add(chat);

        await db.SaveChangesAsync();

        var (ucService, dispatcher) = CreateUCService(alice, db);

        var acceptFriendResult = await ucService.AcceptFriendRequestAsync(bob.AccountId);

        acceptFriendResult.IsSuccess.ShouldBeTrue();
        acceptFriendResult.Value.Message.ShouldNotBeNullOrWhiteSpace();

        // alice should see her connection
        var updatedAlice = await db
            .Users
            .Include(x => x.Connections)
            .SingleAsync(x => x.Id == alice.Id);

        updatedAlice.Friends.ShouldNotBeEmpty();
        updatedAlice.Chats.ShouldNotBeEmpty();
        updatedAlice.Chats.Count.ShouldBe(1);

        // alice should see his connection
        var updatedBob = await db
            .Users
            .Include(x => x.Connections)
            .SingleAsync(x => x.Id == bob.Id);

        updatedBob.Friends.ShouldNotBeEmpty();
        updatedBob.Chats.ShouldNotBeEmpty();
        updatedBob.Chats.Count.ShouldBe(1);

        dispatcher.Notifications.ShouldContain(alice.AccountId);
        dispatcher.Notifications.ShouldContain(bob.AccountId);
    }
}
