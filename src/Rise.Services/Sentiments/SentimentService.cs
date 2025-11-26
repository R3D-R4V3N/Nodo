using Microsoft.EntityFrameworkCore;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Identity;
using Rise.Shared.Sentiments;
using Rise.Shared.UserSentiments;
using Rise.Shared.Common;
using Rise.Services.Sentiments.Mapper;

namespace Rise.Services.Sentiments;

public class SentimentService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider
) : ISentimentsService
{
    public async Task<Result<SentimentResponse.GetSentiments>> GetSentimentsAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        var userId = sessionContextProvider.User!.GetUserId();

        var loggedInUser = await dbContext.Users
            .SingleOrDefaultAsync(x => x.AccountId == userId, ctx);

        if (loggedInUser is null)
            return Result.Unauthorized("U heeft geen toegang om een gevoelens te verkrijgen.");

        var sentimentsQuery = dbContext
            .Sentiments
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            sentimentsQuery = sentimentsQuery.Where(c =>
                c.Category.ToString().Contains(request.SearchTerm));
        }

        var totalCount = await sentimentsQuery.CountAsync(ctx);

        var sentiments = sentimentsQuery
            .AsEnumerable()
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Category)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        return Result.Success(
            new SentimentResponse.GetSentiments
            {
                Sentiments = sentiments.Select(SentimentMapper.ToGetDto),
                TotalCount = totalCount
            }
        );
    }
}
