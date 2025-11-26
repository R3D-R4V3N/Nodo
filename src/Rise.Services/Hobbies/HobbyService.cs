using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Hobbies;
using Rise.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Rise.Shared.Identity;
using Rise.Services.Hobbies.Mapper;

namespace Rise.Services.Hobbies;

public class HobbyService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider
) : IHobbyService
{
    public async Task<Result<HobbyResponse.GetHobbies>> GetHobbiesAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        var userId = sessionContextProvider.User!.GetUserId();

        var loggedInUser = await dbContext.Users
            .SingleOrDefaultAsync(x => x.AccountId == userId, ctx);

        if (loggedInUser is null)
            return Result.Unauthorized("U heeft geen toegang om een hobbies te verkrijgen.");

        var hobbiesQuery = dbContext
            .Hobbies
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            hobbiesQuery = hobbiesQuery.Where(c =>
                c.Hobby.ToString().Contains(request.SearchTerm));
        }

        var totalCount = await hobbiesQuery.CountAsync(ctx);

        var hobbies = hobbiesQuery
            .AsEnumerable()
            .OrderBy(c => c.Hobby)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        return Result.Success(
            new HobbyResponse.GetHobbies
            {
                Hobbies = hobbies.Select(HobbyMapper.ToGetDto),
                TotalCount = totalCount
            }
        );
    }
}
