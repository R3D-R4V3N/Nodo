using Rise.Domain.Events;
using Rise.Domain.Users;
using Rise.Domain.Users.Hobbys;
using Rise.Persistence;
using Rise.Services.Events;
using Rise.Services.Tests.Fakers;
using Rise.Services.Tests.Helpers;
using Rise.Shared.Events;
using Rise.Tests.Shared;
using Rise.Shared.Common;

namespace Rise.Services.Tests.Events;
public class EventServiceTests : IClassFixture<EFFixture>
{
    private EFFixture _fixture;

    public EventServiceTests(EFFixture fixture)
    {
        _fixture = fixture;
    }

    private IEventService CreateEventService(User? loggedInUser, ApplicationDbContext dbcontext)
    {
        if (_fixture is null)
            throw new ArgumentNullException(nameof(_fixture));

        return new EventService(
            dbcontext,
            new FakeSessionContextProvider(ServicesData.GetValidClaimsPrincipal(loggedInUser))
        );
    }

    [Fact]
    public async Task GetEventsAsync_ShouldReturnEvents_WhenThereAreEvents()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser(1);
        db.Users.Add(alice);

        var events = new List<Event>
        {
            DomainData.ValidEvent(1),
            DomainData.ValidEvent(2),
            DomainData.ValidEvent(3),
        };

        db.Events.AddRange(events);

        await db.SaveChangesAsync();

        var eventService = CreateEventService(alice, db);
        var result = await eventService.GetEventsAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(3);
    }

    //[Fact]
    //public async Task GetEventsAsync_ShouldReturnEvents_WhenSkipIsUsed()
    //{
    //    await using var scope = await EfTestScope.CreateScope(_fixture);
    //    var db = scope.DbContext;

    //    var alice = DomainData.ValidUser(1);
    //    db.Users.AddRange(alice);

    //    var events = new List<Event>
    //    {
    //        DomainData.ValidEvent(1),
    //        DomainData.ValidEvent(2),
    //        DomainData.ValidEvent(3),
    //    };

    //    db.Events.AddRange(events);

    //    await db.SaveChangesAsync();

    //    QueryRequest.SkipTake q = new()
    //    {
    //        Skip = 1,
    //    };

    //    var eventService = CreateEventService(alice, db);
    //    var result = await eventService.GetEventsAsync(q);

    //    result.IsSuccess.ShouldBeTrue();
    //    result.Value.Events.Count().ShouldBe(2);
    //    result.Value.TotalCount.ShouldBe(3);
    //}

    //[Fact]
    //public async Task GetEventsAsync_ShouldReturnEvents_WhenSearchTermUsed()
    //{
    //    await using var scope = await EfTestScope.CreateScope(_fixture);
    //    var db = scope.DbContext;

    //    var alice = DomainData.ValidUser(1);
    //    db.Users.AddRange(alice);

    //    var events = new List<Event>
    //    {
    //        DomainData.ValidEvent(1),
    //        DomainData.ValidEvent(2),
    //        DomainData.ValidEvent(3),
    //    };

    //    events[0].Name = "azerty";

    //    db.Events.AddRange(events);

    //    await db.SaveChangesAsync();

    //    QueryRequest.SkipTake q = new()
    //    {
    //        SearchTerm = "azer",
    //    };

    //    var hobbyService = CreateEventService(alice, db);
    //    var result = await hobbyService.GetEventsAsync(q);

    //    result.IsSuccess.ShouldBeTrue();
    //    result.Value.Events.Count().ShouldBe(1);
    //    result.Value.TotalCount.ShouldBe(1);
    //}

    [Fact]
    public async Task GetEventsAsync_ShouldReturnEmpty_WhenNoEvents()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser(1);
        db.Users.AddRange(alice);
        await db.SaveChangesAsync();

        var hobbyService = CreateEventService(alice, db);
        var result = await hobbyService.GetEventsAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetEventsAsync_ShouldUnauthorized_WhenNotLoggedIn()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser(1);

        db.Users.AddRange(alice);
        await db.SaveChangesAsync();

        var hobbyService = CreateEventService(null, db);
        var result = await hobbyService.GetEventsAsync();

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
    }
}
