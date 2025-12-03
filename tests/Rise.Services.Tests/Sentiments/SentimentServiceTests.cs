using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Tests.Fakers;
using Rise.Services.Tests.Helpers;
using Rise.Tests.Shared;
using Rise.Shared.Common;
using Rise.Shared.UserSentiments;
using Rise.Services.Sentiments;
using Rise.Domain.Users.Sentiment;

namespace Rise.Services.Tests.Sentiments;

public class SentimentServiceTests : IClassFixture<EFFixture>
{
    private EFFixture _fixture;

    public SentimentServiceTests(EFFixture fixture)
    {
        _fixture = fixture;
    }

    private ISentimentsService CreateSentimentService(User? loggedInUser, ApplicationDbContext dbcontext)
    {
        if (_fixture is null)
            throw new ArgumentNullException(nameof(_fixture));

        return new SentimentService(
            dbcontext,
            new FakeSessionContextProvider(ServicesData.GetValidClaimsPrincipal(loggedInUser))
        );
    }

    [Fact]
    public async Task GetSentimentsAsync_ShouldReturnSentiments_WhenThereAreSentiments()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.Add(alice);

        List<UserSentiment> sentiments = [
            new UserSentiment
            {
                Type = SentimentType.Like,
                Category = SentimentCategoryType.TravelAdventures
            },
            new UserSentiment
            {
                Type = SentimentType.Dislike,
                Category = SentimentCategoryType.TravelAdventures
            },
            new UserSentiment
            {
                Type = SentimentType.Like,
                Category = SentimentCategoryType.ActionMovies
            },
            new UserSentiment
            {
                Type = SentimentType.Dislike,
                Category = SentimentCategoryType.ActionMovies
            },
        ];

        db.Sentiments.AddRange(sentiments);

        await db.SaveChangesAsync();

        var hobbyService = CreateSentimentService(alice, db);
        var result = await hobbyService.GetSentimentsAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(4);
    }

    [Fact]
    public async Task GetSentimetnsAsync_ShouldReturnHobbies_WhenSkipIsUsed()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.AddRange(alice);

        List<UserSentiment> sentiments = [
            new UserSentiment
            {
                Type = SentimentType.Like,
                Category = SentimentCategoryType.TravelAdventures
            },
            new UserSentiment
            {
                Type = SentimentType.Dislike,
                Category = SentimentCategoryType.TravelAdventures
            },
            new UserSentiment
            {
                Type = SentimentType.Like,
                Category = SentimentCategoryType.ActionMovies
            },
            new UserSentiment
            {
                Type = SentimentType.Dislike,
                Category = SentimentCategoryType.ActionMovies
            },
        ];

        db.Sentiments.AddRange(sentiments);

        await db.SaveChangesAsync();

        QueryRequest.SkipTake q = new()
        {
            Skip = 1,
        };
        var sentimentService = CreateSentimentService(alice, db);
        var result = await sentimentService.GetSentimentsAsync(q);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Sentiments.Count().ShouldBe(3);
        result.Value.TotalCount.ShouldBe(4);
    }

    [Fact]
    public async Task GetSentimentsAsync_ShouldReturnHobbies_WhenSearchTermUsed()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.AddRange(alice);

        List<UserSentiment> sentiments = [
            new UserSentiment
            {
                Type = SentimentType.Like,
                Category = SentimentCategoryType.TravelAdventures
            },
            new UserSentiment
            {
                Type = SentimentType.Dislike,
                Category = SentimentCategoryType.TravelAdventures
            },
            new UserSentiment
            {
                Type = SentimentType.Like,
                Category = SentimentCategoryType.ActionMovies
            },
            new UserSentiment
            {
                Type = SentimentType.Dislike,
                Category = SentimentCategoryType.ActionMovies
            },
        ];

        db.Sentiments.AddRange(sentiments);

        await db.SaveChangesAsync();

        QueryRequest.SkipTake q = new()
        {
            SearchTerm = "mov",
        };
        var sentimentService = CreateSentimentService(alice, db);
        var result = await sentimentService.GetSentimentsAsync(q);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Sentiments.Count().ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetSentimentsAsync_ShouldReturnEmpty_WhenNoSentiments()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();
        db.Users.AddRange(alice);
        await db.SaveChangesAsync();

        var hobbyService = CreateSentimentService(alice, db);
        var result = await hobbyService.GetSentimentsAsync(new());

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetSentimentsAsync_ShouldUnauthorized_WhenNotLoggedIn()
    {
        await using var scope = await EfTestScope.CreateScope(_fixture);
        var db = scope.DbContext;

        var alice = DomainData.ValidUser();

        db.Users.AddRange(alice);
        await db.SaveChangesAsync();

        var hobbyService = CreateSentimentService(null, db);
        var result = await hobbyService.GetSentimentsAsync(new());

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Unauthorized);
    }
}
