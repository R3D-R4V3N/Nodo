using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Tests.Fakers;
using Rise.Services.Tests.Helpers;
using Rise.Services.Hobbies;
using Rise.Shared.Hobbies;
using Rise.Tests.Shared;
using Rise.Domain.Users.Hobbys;
using Rise.Shared.Common;

namespace Rise.Services.Tests.Hobbies;

public class HobbyServiceTests : IClassFixture<EFFixture>
{
    private EFFixture _fixture;

    public HobbyServiceTests(EFFixture fixture)
    {
        _fixture = fixture;
    }

    private IHobbyService CreateHobbyService(User? loggedInUser, ApplicationDbContext dbcontext)
    {
        if (_fixture is null)
            throw new ArgumentNullException(nameof(_fixture));

        return new HobbyService(
            dbcontext,
            new FakeSessionContextProvider(ServicesData.GetValidClaimsPrincipal(loggedInUser))
        );
    }

    [Fact]
    public async Task GetHobbiesAsync_ShouldReturnHobbies_WhenThereAreHobbies()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.Add(alice);

        List<UserHobby> hobbies = [
            new UserHobby
            {
                Hobby = HobbyType.Pilates
            },
            new UserHobby
            {
                Hobby = HobbyType.Puzzles
            },
            new UserHobby
            {
                Hobby = HobbyType.Basketball
            }
        ];

        db.Hobbies.AddRange(hobbies);

        await db.SaveChangesAsync();

        var hobbyService = CreateHobbyService(alice, db);
        var result = await hobbyService.GetHobbiesAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(3);
    }

    [Fact]
    public async Task GetHobbiesAsync_ShouldReturnHobbies_WhenSkipIsUsed()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.AddRange(alice);

        List<UserHobby> hobbies = [
            new UserHobby
            {
                Hobby = HobbyType.Pilates
            },
            new UserHobby
            {
                Hobby = HobbyType.Puzzles
            },
            new UserHobby
            {
                Hobby = HobbyType.Basketball
            }
        ];
        db.Hobbies.AddRange(hobbies);

        await db.SaveChangesAsync();

        QueryRequest.SkipTake q = new()
        {
            Skip = 1,
        };
        var hobbyService = CreateHobbyService(alice, db);
        var result = await hobbyService.GetHobbiesAsync(q);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Hobbies.Count().ShouldBe(2);
        result.Value.TotalCount.ShouldBe(3);
    }

    [Fact]
    public async Task GetHobbiesAsync_ShouldReturnHobbies_WhenSearchTermUsed()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.AddRange(alice);

        List<UserHobby> hobbies = [
            new UserHobby
            {
                Hobby = HobbyType.Pilates
            },
            new UserHobby
            {
                Hobby = HobbyType.Puzzles
            },
            new UserHobby
            {
                Hobby = HobbyType.Basketball
            }
        ];
        db.Hobbies.AddRange(hobbies);

        await db.SaveChangesAsync();

        QueryRequest.SkipTake q = new()
        {
            SearchTerm = "pil",
        };
        var hobbyService = CreateHobbyService(alice, db);
        var result = await hobbyService.GetHobbiesAsync(q);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Hobbies.Count().ShouldBe(1);
        result.Value.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetHobbiesAsync_ShouldReturnEmpty_WhenNoHobbies()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.AddRange(alice);
        await db.SaveChangesAsync();

        var hobbyService = CreateHobbyService(alice, db);
        var result = await hobbyService.GetHobbiesAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetHobbiesAsync_ShouldUnauthorized_WhenNotLoggedIn()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();

        db.Users.AddRange(alice);
        await db.SaveChangesAsync();

        var hobbyService = CreateHobbyService(null, db);
        var result = await hobbyService.GetHobbiesAsync(new());

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
    }
}
