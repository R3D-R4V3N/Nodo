using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;
using Rise.Persistence;
using Rise.Shared.Sentiments;

namespace Rise.Services.Sentiments.Mapper;

internal static class SentimentMapper
{
    public static SentimentDto.Get ToGetDto(UserSentiment sentiment)
    {
        return new SentimentDto.Get()
        {
            Id = sentiment.Id,
            Type = sentiment.Type.ToDto(),
            Category = sentiment.Category.ToDto(),
        };
    }

    public static async Task<Result<UserSentiment>> ToDomainAsync(
        SentimentDto.EditProfile sentimentDto,
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        if (sentimentDto is null)
        {
            return Result.Invalid(new ValidationError(nameof(SentimentDto), $"Lege sentiment meegegeven."));
        }

        SentimentType type = sentimentDto.Type.ToDomain();
        SentimentCategoryType category = sentimentDto.Category.ToDomain();

        var sentiment = await dbContext
            .Sentiments
            .FirstOrDefaultAsync(x => x.Type.Equals(type) && x.Category.Equals(category), ct);

        if (sentiment is null)
        {
            return Result.Conflict($"Onbekende hobby {sentimentDto}");
        }

        return Result.Success(sentiment);
    }

    public static async Task<Result<List<UserSentiment>>> ToDomainAsync(
        IEnumerable<SentimentDto.EditProfile> sentimentDtos,
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        if (sentimentDtos is null)
        {
            return Result.Success(new List<UserSentiment>());
        }

        var sentiments = new List<UserSentiment>();

        foreach (var sentimentDto in sentimentDtos)
        {
            var result = await ToDomainAsync(sentimentDto, dbContext, ct);

            if (!result.IsSuccess)
            {
                if (result.ValidationErrors.Any())
                {
                    return Result.Invalid(result.ValidationErrors);
                }
                return Result.Conflict(result.Errors.ToArray());
            }

            sentiments.Add(result.Value);
        }

        return Result.Success(sentiments);
    }
}
