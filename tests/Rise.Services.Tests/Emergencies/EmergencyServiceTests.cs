using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Emergencies;
using Rise.Domain.Events;
using Rise.Domain.Users;
using Rise.Domain.Users.Hobbys;
using Rise.Persistence;
using Rise.Services.Emergencies;
using Rise.Services.Emergencies.Mapper;
using Rise.Services.Events;
using Rise.Services.Tests.Fakers;
using Rise.Services.Tests.Helpers;
using Rise.Shared.Common;
using Rise.Shared.Emergencies;
using Rise.Shared.Events;
using Rise.Tests.Shared;
using Shouldly;

namespace Rise.Services.Tests.Emergencies;
public class EmergencyServiceTests : IClassFixture<EFFixture>
{
    private EFFixture _fixture;

    public EmergencyServiceTests(EFFixture fixture)
    {
        _fixture = fixture;
    }

    private IEmergencyService CreateEmergencyService(BaseUser? loggedInUser, ApplicationDbContext dbcontext)
    {
        if (_fixture is null)
            throw new ArgumentNullException(nameof(_fixture));

        return new EmergencyService(
            dbcontext,
            new FakeSessionContextProvider(ServicesData.GetValidClaimsPrincipal(loggedInUser))
        );
    }

    [Fact]
    public async Task CreateEmergencyAsync_ShouldCreate_WhenThereIsNoRecentEmergency()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        alice.SendFriendRequest(bob);
        bob.AcceptFriendRequest(alice);
        var chat = Chat.CreatePrivateChat(alice, bob).Value;
        var message = chat.AddTextMessage("This is a test.", bob).Value;

        db.Users.AddRange(alice, bob);
        db.Chats.Add(chat);

        await db.SaveChangesAsync();

        var emergencyService = CreateEmergencyService(alice, db);

        var request = new EmergencyRequest.CreateEmergency() 
        {
            ChatId = chat.Id,
            MessageId = message.Id,
            Type = EmergencyTypeDto.Other,
        };

        var result = await emergencyService.CreateEmergencyAsync(request);

        result.IsSuccess.ShouldBeTrue();

        var emergencyDb = await db
            .Emergencies
            .Include(e => e.MadeByUser)
            .Include(e => e.HappenedInChat)
            .SingleAsync();

        emergencyDb.ShouldNotBeNull();
        emergencyDb.MadeByUser.ShouldBe(alice);
        emergencyDb.HappenedInChat.ShouldBe(chat);
    }

    [Fact]
    public async Task CreateEmergencyAsync_ShouldConflict_WhenThereIsRecentEmergency()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();

        alice.SendFriendRequest(bob);
        bob.AcceptFriendRequest(alice);
        var chat = Chat.CreatePrivateChat(alice, bob).Value;
        var message = chat.AddTextMessage("This is a test.", bob).Value;

        var emergency = new Emergency
        {
            HappenedInChat = chat,
            MadeByUser = alice,
            Range = EmergencyRange.Create(DateTime.UtcNow),
            Type = EmergencyType.Other
        };

        db.Users.AddRange(alice, bob);
        db.Chats.Add(chat);
        db.Emergencies.Add(emergency);

        await db.SaveChangesAsync();

        var emergencyService = CreateEmergencyService(alice, db);

        var request = new EmergencyRequest.CreateEmergency()
        {
            ChatId = chat.Id,
            MessageId = message.Id,
            Type = EmergencyTypeDto.Other,
        };

        var result = await emergencyService.CreateEmergencyAsync(request);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldContain("Er werd al recent een noodmelding gestuurd.");
    }

    [Fact]
    public async Task CreateEmergencyAsync_ShouldUnauth_WhenNotInchat()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        var bob = DomainData.ValidUser();
        var john = DomainData.ValidUser();

        alice.SendFriendRequest(bob);
        bob.AcceptFriendRequest(alice);
        var chat = Chat.CreatePrivateChat(alice, bob).Value;
        var message = chat.AddTextMessage("This is a test.", bob).Value;

        db.Users.AddRange(alice, bob, john);
        db.Chats.Add(chat);

        await db.SaveChangesAsync();

        var emergencyService = CreateEmergencyService(john, db);

        var request = new EmergencyRequest.CreateEmergency()
        {
            ChatId = chat.Id,
            MessageId = message.Id,
            Type = EmergencyTypeDto.Other,
        };

        var result = await emergencyService.CreateEmergencyAsync(request);

        result.Status.ShouldBe(ResultStatus.Unauthorized);
        result.Errors.ShouldContain("Geen toegang tot deze chat.");
    }

    // no time left for more create test

    [Fact]
    public async Task GetEmergenciesAsync_ShouldReturn_WhenSkipIsUsed()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidSupervisor();

        var user1 = DomainData.ValidUser();
        user1.Supervisor = alice;
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        var chat = Chat.CreatePrivateChat(user1, user2).Value;
        var message = chat.AddTextMessage("test", user1).Value;

        var emergencies = new List<Emergency>
        {
            new Emergency
            {
                HappenedInChat = chat,
                MadeByUser = user1,
                Range = EmergencyRange.Create(DateTime.UtcNow),
                Type = EmergencyType.Other
            },
            new Emergency
            {
                HappenedInChat = chat,
                MadeByUser = user1,
                Range = EmergencyRange.Create(DateTime.UtcNow),
                Type = EmergencyType.Other
            },
            new Emergency
            {
                HappenedInChat = chat,
                MadeByUser = user1,
                Range = EmergencyRange.Create(DateTime.UtcNow),
                Type = EmergencyType.Other
            },
        };

        db.Supervisors.AddRange(alice);
        db.Users.AddRange(user1, user2);
        db.Chats.Add(chat);
        db.Emergencies.AddRange(emergencies);

        await db.SaveChangesAsync();

        QueryRequest.SkipTake q = new()
        {
            Skip = 1,
        };

        var eventService = CreateEmergencyService(alice, db);
        var result = await eventService.GetEmergenciesAsync(q);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Emergencies.Count().ShouldBe(2);
        result.Value.TotalCount.ShouldBe(3);
    }

    [Fact]
    public async Task GetEmergenciesAsync_ShouldReturn_WhenSearchTermUsed()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidSupervisor();

        var user1 = DomainData.ValidUser();
        user1.Supervisor = alice;
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        var chat = Chat.CreatePrivateChat(user1, user2).Value;
        var message = chat.AddTextMessage("test", user1).Value;

        var emergencies = new List<Emergency>
        {
            new Emergency
            {
                HappenedInChat = chat,
                MadeByUser = user1,
                Range = EmergencyRange.Create(DateTime.UtcNow),
                Type = EmergencyType.Other
            },
            new Emergency
            {
                HappenedInChat = chat,
                MadeByUser = user2,
                Range = EmergencyRange.Create(DateTime.UtcNow),
                Type = EmergencyType.Other
            },
            new Emergency
            {
                HappenedInChat = chat,
                MadeByUser = user2,
                Range = EmergencyRange.Create(DateTime.UtcNow),
                Type = EmergencyType.Other
            },
        };

        emergencies.First().MadeByUser.FirstName = FirstName.Create("te");

        db.Supervisors.AddRange(alice);
        db.Users.AddRange(user1, user2);
        db.Chats.Add(chat);
        db.Emergencies.AddRange(emergencies);
        await db.SaveChangesAsync();

        QueryRequest.SkipTake q = new()
        {
            SearchTerm = "te",
        };

        var hobbyService = CreateEmergencyService(alice, db);
        var result = await hobbyService.GetEmergenciesAsync(q);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Emergencies.Count().ShouldBe(1);
        result.Value.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetEmergenciesAsync_ShouldReturnEmpty_WhenNoEvents()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidSupervisor();
        db.Supervisors.AddRange(alice);
        await db.SaveChangesAsync();

        var hobbyService = CreateEmergencyService(alice, db);
        var result = await hobbyService.GetEmergenciesAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetEventsAsync_ShouldUnauthorized_WhenRegularuser()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();

        db.Users.AddRange(alice);
        await db.SaveChangesAsync();

        var hobbyService = CreateEmergencyService(alice, db);
        var result = await hobbyService.GetEmergenciesAsync(new());

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task GetEventsAsync_ShouldUnauthorized_WhenNotLoggedIn()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();

        db.Users.AddRange(alice);
        await db.SaveChangesAsync();

        var hobbyService = CreateEmergencyService(null, db);
        var result = await hobbyService.GetEmergenciesAsync(new());

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
    }

   // [Fact]
    public async Task GetEmergencyAsync_ShouldReturn()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidSupervisor();

        var user1 = DomainData.ValidUser();
        user1.Supervisor = alice;
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        var chat = Chat.CreatePrivateChat(user1, user2).Value;
        var message = chat.AddTextMessage("test", user1).Value;

        var emergency = new Emergency
        {
            HappenedInChat = chat,
            MadeByUser = user1,
            Range = EmergencyRange.Create(DateTime.UtcNow),
            Type = EmergencyType.Other
        };

        db.Supervisors.AddRange(alice);
        db.Users.AddRange(user1, user2);
        db.Chats.Add(chat);
        db.Emergencies.Add(emergency);

        await db.SaveChangesAsync();

        var eventService = CreateEmergencyService(alice, db);
        var result = await eventService.GetEmergencyAsync(emergency.Id);

        result.IsSuccess.ShouldBeTrue();

        var updatedChat = await db
            .Chats
            .Include(x => x.Emergencies)
            .SingleAsync(x => x.Id == emergency.HappenedInChat.Id);

        updatedChat.Emergencies.ShouldNotBeEmpty();
    }
}
