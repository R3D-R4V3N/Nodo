using Ardalis.Result;
using EmptyFiles;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Users;
using Rise.Domain.Users.Connections;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;
using Rise.Persistence;
using Rise.Services.Tests.Fakers;
using Rise.Services.Tests.Helpers;
using Rise.Services.UserConnections;
using Rise.Services.UserConnections.Mapper;
using Rise.Shared.Common;
using Rise.Shared.UserConnections;
using Rise.Tests.Shared;
using System.Transactions;

namespace Rise.Services.Tests.UserConnections;
public class UserConnectionServiceTests : IClassFixture<EFFixture>
{
    private EFFixture _fixture;

    public UserConnectionServiceTests(EFFixture fixture)
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
    public async Task GetFriendsAsync_ShouldReturnFriends_WhenThereAreFriends()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        alice.SendFriendRequest(bob);
        bob.AcceptFriendRequest(alice);
        
        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);
        var result = await ucService.GetFriendsAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetFriendsAsync_ShouldReturnFriends_WhenSkipIsUsed()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();
        var john = DomainData.ValidUser();

        alice.SendFriendRequest(bob);
        bob.AcceptFriendRequest(alice);

        alice.SendFriendRequest(john);
        john.AcceptFriendRequest(alice);

        db.Users.AddRange(alice, bob, john);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);
        QueryRequest.SkipTake q = new()
        { 
            Skip = 1,
        };
        var result = await ucService.GetFriendsAsync(q);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Connections.Count().ShouldBe(1);
        result.Value.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetFriendsAsync_ShouldReturnFriends_WhenSearchTermUsed()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();
        var john = DomainData.ValidUser();
        bob.FirstName = FirstName.Create("bob");

        alice.SendFriendRequest(bob);
        bob.AcceptFriendRequest(alice);

        alice.SendFriendRequest(john);
        john.AcceptFriendRequest(alice);

        db.Users.AddRange(alice, bob, john);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);
        QueryRequest.SkipTake q = new()
        {
            SearchTerm = bob.FirstName
        };
        var result = await ucService.GetFriendsAsync(q);

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetFriendsAsync_ShouldReturnEmpty_WhenNoFriends()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();

        db.Users.AddRange(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);
        var result = await ucService.GetFriendsAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.Connections.ShouldBeEmpty();
        result.Value.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetFriendsAsync_ShouldUnauthorized_WhenNotLoggedIn()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();

        db.Users.AddRange(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(null, db);
        var result = await ucService.GetFriendsAsync(new());

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task GetFriendRequestsAsync_ShouldReturnFriendRequests_WhenThereAreFriendRequests()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();
        var john = DomainData.ValidUser();

        alice.SendFriendRequest(bob);
        john.SendFriendRequest(alice);

        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);
        var result = await ucService.GetFriendRequestsAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetFriendRequestsAsync_ShouldReturnFriendRequests_WhenSkipIsUsed()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();
        var john = DomainData.ValidUser();

        alice.SendFriendRequest(bob);
        john.SendFriendRequest(alice);

        db.Users.AddRange(alice, bob, john);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);
        QueryRequest.SkipTake q = new()
        {
            Skip = 1,
        };
        var result = await ucService.GetFriendRequestsAsync(q);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Connections.Count().ShouldBe(1);
        result.Value.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetFriendRequestsAsync_ShouldReturnFriendRequests_WhenSearchTermUsed()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();
        var john = DomainData.ValidUser();
        bob.FirstName = FirstName.Create("bob");

        alice.SendFriendRequest(bob);
        john.SendFriendRequest(alice);

        db.Users.AddRange(alice, bob, john);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);
        QueryRequest.SkipTake q = new()
        {
            SearchTerm = bob.FirstName
        };
        var result = await ucService.GetFriendRequestsAsync(q);

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetFriendRequestsAsync_ShouldReturnEmpty_WhenNoFriendRequests()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();

        db.Users.AddRange(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);
        var result = await ucService.GetFriendRequestsAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.Connections.ShouldBeEmpty();
        result.Value.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetFriendRequestsAsync_ShouldUnauthorized_WhenNotLoggedIn()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();

        db.Users.AddRange(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(null, db);
        var result = await ucService.GetFriendRequestsAsync(new());

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task GetSuggestedFriendsAsync_ShouldReturnUsers_WhenCandidatesExist()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();
        var john = DomainData.ValidUser();

        db.Users.AddRange(alice, bob, john);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.GetSuggestedFriendsAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetSuggestedFriendsAsync_ShouldReturnZero_WhenNoOtherUsers()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.Add(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.GetSuggestedFriendsAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(0);
        result.Value.Users.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetSuggestedFriendsAsync_ShouldNotReturnUsersWithExistingConnections()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();
        var john = DomainData.ValidUser();

        alice.SendFriendRequest(bob); 
        db.Users.AddRange(alice, bob, john);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.GetSuggestedFriendsAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(1);
        result.Value.Users.Single().User.AccountId.ShouldBe(john.AccountId);
    }

    [Fact]
    public async Task GetSuggestedFriendsAsync_ShouldRankBySharedLikesAndHobbies()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var movies = new UserSentiment
        {
            Type = SentimentType.Like,
            Category = SentimentCategoryType.ActionMovies
        };

        var pilates = new UserHobby
        {
            Hobby = HobbyType.Pilates
        };

        var basket = new UserHobby
        {
            Hobby = HobbyType.Basketball
        };

        db.Sentiments.Add(movies);
        db.Hobbies.AddRange(pilates, basket);

        var alice = DomainData.ValidUser();
        alice.UpdateSentiments([movies]);
        alice.UpdateHobbies([pilates]);

        var bob = DomainData.ValidUser();
        bob.UpdateSentiments([movies]);
        bob.UpdateHobbies([basket]);

        var john = DomainData.ValidUser();
        john.UpdateSentiments([movies]);
        john.UpdateHobbies([pilates]);

        db.Users.AddRange(alice, bob, john);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.GetSuggestedFriendsAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.Users.First().User.AccountId.ShouldBe(john.AccountId);
    }

    [Fact]
    public async Task GetSuggestedFriendsAsync_ShouldRespectSkipAndTake()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var users = Enumerable.Range(2, 5).Select(_ => DomainData.ValidUser()).ToList();

        db.Users.Add(alice);
        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.GetSuggestedFriendsAsync(new QueryRequest.SkipTake
        {
            Skip = 1,
            Take = 2
        });

        result.IsSuccess.ShouldBeTrue();
        result.Value.Users.Count().ShouldBe(2);
        result.Value.TotalCount.ShouldBe(5);
    }

    [Fact]
    public async Task GetSuggestedFriendsAsync_ShouldReturnUnauthorized_WhenUserNotFound()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.GetSuggestedFriendsAsync(new());

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task CancelFriendRequest_ShouldCancel_WhenRequestExists()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser(); 
        var bob = DomainData.ValidUser();
        alice.SendFriendRequest(bob);

        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var (ucService, dispatcher) = CreateUCService(alice, db);

        var result = await ucService.CancelFriendRequest(bob.AccountId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Message.ShouldNotBeNullOrWhiteSpace();

        // alice shouldnt see her deleted connection
        var updatedAlice = await db
            .Users
            .Include(x => x.Connections)
            .SingleAsync(x => x.Id == alice.Id);

        updatedAlice.Connections.ShouldBeEmpty();

        // bob shouldnt see his deleted connection
        var updatedBob = await db
            .Users
            .Include(x => x.Connections)
            .SingleAsync(x => x.Id == bob.Id);

        updatedBob.Connections.ShouldBeEmpty();

        // it should be a soft delete in db
        var userConnections = await db
            .UserConnections
            .IgnoreQueryFilters()
            .Include(x => x.From)
            .Include(x => x.To)
            .ToListAsync();

        userConnections.ShouldNotBeEmpty();
        dispatcher.Notifications.ShouldContain(alice.AccountId);
        dispatcher.Notifications.ShouldContain(bob.AccountId);
    }

    [Fact]
    public async Task CancelFriendRequest_ShouldUnauthorized_WhenNotLoggedIn()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.Add(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(null, db);

        var result = await ucService.CancelFriendRequest(alice.AccountId);

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
        result.Errors.ShouldContain("U heeft geen toegang om een vriendschapsverzoek te weigeren.");
    }

    [Fact]
    public async Task CancelFriendRequest_ShouldReturnNotFound_WhenConnectionMissing()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        db.Users.Add(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.CancelFriendRequest(bob.AccountId);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Connectie niet gevonden.");
    }

    [Fact]
    public async Task CancelFriendRequest_ShouldReturnConflict_WhenDomainPreventsCancellation()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        alice.SendFriendRequest(bob);
        bob.AcceptFriendRequest(alice);

        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.CancelFriendRequest(bob.AccountId);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Conflict);

        var connections = await db
            .UserConnections
            .ToListAsync();

        connections.Count.ShouldBe(2);
    }

    [Fact]
    public async Task RemoveFriendAsync_ShouldRemove_WhenFriendshipExists()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();
        alice.SendFriendRequest(bob);
        bob.AcceptFriendRequest(alice);

        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var (ucService, dispatcher) = CreateUCService(alice, db);

        var result = await ucService.RemoveFriendAsync(bob.AccountId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Message.ShouldNotBeNullOrWhiteSpace();

        // alice shouldnt see her deleted connection
        var updatedAlice = await db
            .Users
            .Include(x => x.Connections)
            .SingleAsync(x => x.Id == alice.Id);

        updatedAlice.Connections.ShouldBeEmpty();

        // bob shouldnt see his deleted connection
        var updatedBob = await db
            .Users
            .Include(x => x.Connections)
            .SingleAsync(x => x.Id == bob.Id);

        updatedBob.Connections.ShouldBeEmpty();

        // it should be a soft delete in db
        var userConnections = await db
            .UserConnections
            .IgnoreQueryFilters()
            .Include(x => x.From)
            .Include(x => x.To)
            .ToListAsync();

        userConnections.ShouldNotBeEmpty();
        dispatcher.Notifications.ShouldContain(alice.AccountId);
        dispatcher.Notifications.ShouldContain(bob.AccountId);
    }

    [Fact]
    public async Task RemoveFriendAsync_ShouldUnauthorized_WhenNotLoggedIn()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.Add(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(null, db);

        var result = await ucService.RemoveFriendAsync(alice.AccountId);

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
        result.Errors.ShouldContain("U heeft geen toegang om een vriendschap te verwijderen.");
    }

    [Fact]
    public async Task RemoveFriendAsync_ShouldReturnNotFound_WhenConnectionMissing()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        db.Users.Add(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.RemoveFriendAsync(bob.AccountId);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Connectie niet gevonden.");
    }

    [Fact]
    public async Task RemoveFriendAsync_ShouldReturnConflict_WhenDomainPreventsRemoval()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        alice.SendFriendRequest(bob);

        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.RemoveFriendAsync(bob.AccountId);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Conflict);

        var connections = await db
            .UserConnections
            .ToListAsync();

        connections.Count.ShouldBe(2);
    }

    [Fact]
    public async Task SendFriendRequestAsync_ShouldSend_WhenNoConnection()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var (ucService, dispatcher) = CreateUCService(alice, db);

        var result = await ucService.SendFriendRequestAsync(bob.AccountId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Message.ShouldNotBeNullOrWhiteSpace();

        // alice should see her connections
        var updatedAlice = await db
            .Users
            .Include(x => x.Connections)
            .SingleAsync(x => x.Id == alice.Id);

        updatedAlice.Connections.ShouldNotBeEmpty();

        // bob should see his connection
        var updatedBob = await db
            .Users
            .Include(x => x.Connections)
            .SingleAsync(x => x.Id == bob.Id);

        updatedBob.Connections.ShouldNotBeEmpty();
        dispatcher.Notifications.ShouldContain(alice.AccountId);
        dispatcher.Notifications.ShouldContain(bob.AccountId);
    }

    [Fact]
    public async Task SendFriendRequestAsync_ShouldUnauthorized_WhenNotLoggedIn()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.Add(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(null, db);

        var result = await ucService.SendFriendRequestAsync(alice.AccountId);

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
        result.Errors.ShouldContain("U heeft geen toegang om een verzoek te verzenden.");
    }

    [Fact]
    public async Task SendFriendRequestAsync_ShouldReturnNotFound_WhenTargetUserMissing()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        db.Users.Add(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.SendFriendRequestAsync(bob.AccountId);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Gebruiker niet gevonden.");
    }

    [Fact]
    public async Task SendFriendRequestAsync_ShouldReturnConflict_WhenDomainPreventsRemoval()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        alice.SendFriendRequest(bob);

        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.SendFriendRequestAsync(bob.AccountId);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Conflict);

        var connections = await db
            .UserConnections
            .ToListAsync();

        connections.Count.ShouldBe(2);
    }

    [Fact]
    public async Task AcceptFriendRequestAsync_ShouldUnauthorized_WhenNotLoggedIn()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.Add(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(null, db);

        var result = await ucService.AcceptFriendRequestAsync(alice.AccountId);

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
        result.Errors.ShouldContain("U heeft geen toegang om een vriendschap te accepteren.");
    }

    [Fact]
    public async Task AcceptFriendRequestAsync_ShouldReturnNotFound_WhenConnectionMissing()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        db.Users.Add(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.AcceptFriendRequestAsync(bob.AccountId);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Connectie niet gevonden.");
    }

    [Fact]
    public async Task AcceptFriendRequestAsync_ShouldReturnConflict_WhenDomainPreventsRemoval()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        alice.SendFriendRequest(bob);
        alice.AcceptFriendRequest(bob);

        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.AcceptFriendRequestAsync(bob.AccountId);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Conflict);

        var connections = await db
            .UserConnections
            .ToListAsync();

        connections.Count.ShouldBe(2);
    }

    [Fact]
    public async Task RejectFriendRequestAsync_ShouldReject_WhenRequestExists()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();
        bob.SendFriendRequest(alice);

        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var (ucService, dispatcher) = CreateUCService(alice, db);

        var result = await ucService.RejectFriendRequestAsync(bob.AccountId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Message.ShouldNotBeNullOrWhiteSpace();

        // alice shouldnt see her deleted connection
        var updatedAlice = await db
            .Users
            .Include(x => x.Connections)
            .SingleAsync(x => x.Id == alice.Id);

        updatedAlice.Connections.ShouldBeEmpty();

        // bob shouldnt see his deleted connection
        var updatedBob = await db
            .Users
            .Include(x => x.Connections)
            .SingleAsync(x => x.Id == bob.Id);

        updatedBob.Connections.ShouldBeEmpty();

        // it should be a soft delete in db
        var userConnections = await db
            .UserConnections
            .IgnoreQueryFilters()
            .Include(x => x.From)
            .Include(x => x.To)
            .ToListAsync();

        userConnections.ShouldNotBeEmpty();
        dispatcher.Notifications.ShouldContain(alice.AccountId);
        dispatcher.Notifications.ShouldContain(bob.AccountId);
    }

    [Fact]
    public async Task RejectFriendRequestAsync_ShouldUnauthorized_WhenNotLoggedIn()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.Add(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(null, db);

        var result = await ucService.RejectFriendRequestAsync(alice.AccountId);

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
        result.Errors.ShouldContain("U heeft geen toegang om een vriendschap te wijgeren.");
    }

    [Fact]
    public async Task RejectFriendRequestAsync_ShouldReturnNotFound_WhenConnectionMissing()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        db.Users.Add(alice);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.RejectFriendRequestAsync(bob.AccountId);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Connectie niet gevonden.");
    }

    [Fact]
    public async Task RejectFriendRequestAsync_ShouldReturnConflict_WhenDomainPreventsRemoval()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        alice.SendFriendRequest(bob);

        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var (ucService, _) = CreateUCService(alice, db);

        var result = await ucService.RejectFriendRequestAsync(bob.AccountId);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Conflict);

        var connections = await db
            .UserConnections
            .ToListAsync();

        connections.Count.ShouldBe(2);
    }
}
