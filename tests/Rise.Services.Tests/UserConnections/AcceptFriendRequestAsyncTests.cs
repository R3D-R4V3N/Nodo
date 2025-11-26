using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Tests.Fakers;
using Rise.Services.Tests.Helpers;
using Rise.Services.UserConnections;
using Rise.Shared.UserConnections;
using Rise.Tests.Shared;
using System.Transactions;

namespace Rise.Services.Tests.UserConnections;
public class AcceptFriendRequestAsyncTests : IClassFixture<EFFixture>
{
    private EFFixture _fixture;

    public AcceptFriendRequestAsyncTests(EFFixture fixture)
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

        var alice = DomainData.ValidUser(1);
        var bob = DomainData.ValidUser(2);
        bob.SendFriendRequest(alice);
 
        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var (ucService, dispatcher) = CreateUCService(alice, db);

        var result = await ucService.AcceptFriendRequestAsync(bob.AccountId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Message.ShouldNotBeNullOrWhiteSpace();

        // alice should see her connection
        var updatedAlice = await db
            .Users
            .Include(x => x.Connections)
            .SingleAsync(x => x.Id == alice.Id);

        updatedAlice.Friends.ShouldNotBeEmpty();
        updatedAlice.Chats.ShouldNotBeEmpty();

        // alice should see his connection
        var updatedBob = await db
            .Users
            .Include(x => x.Connections)
            .SingleAsync(x => x.Id == bob.Id);

        updatedBob.Friends.ShouldNotBeEmpty();
        updatedBob.Chats.ShouldNotBeEmpty();


        dispatcher.Notifications.ShouldContain(alice.AccountId);
        dispatcher.Notifications.ShouldContain(bob.AccountId);
    }
}
