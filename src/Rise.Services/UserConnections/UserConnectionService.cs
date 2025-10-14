using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.UserConnections.Mapper;
using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.UserConnections;

namespace Rise.Services.UserConnections;
/// <summary>
/// Service for user connections. Note the use of <see cref="ISessionContextProvider"/> to get the current user in this layer of the application.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="sessionContextProvider"></param>
public class UserConnectionService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider) : IUserConnectionService
{
    public async Task<Result<UserConnectionResponse.Index>> GetFriendIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        var userId = sessionContextProvider.User!.GetUserId();

        var loggedInUser = await dbContext.ApplicationUsers
            .SingleOrDefaultAsync(x => x.AccountId == sessionContextProvider.User!.GetUserId(), ctx);

        if (loggedInUser is null)
            return Result.Unauthorized("You are not authorized to fetch user connections.");

        var query = dbContext.ApplicationUsers
            .Where(u => u.AccountId == userId)
            .SelectMany(u => EF.Property<IEnumerable<UserConnection>>(u, "_connections"))
            .Where(c => 
                c.ConnectionType.Equals(UserConnectionType.Friend)
                || c.ConnectionType.Equals(UserConnectionType.RequestIncoming)
                || c.ConnectionType.Equals(UserConnectionType.RequestOutgoing)
            )
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p => p.Connection.FirstName.Contains(request.SearchTerm) || p.Connection.LastName.Contains(request.SearchTerm));
        }

        var totalCount = await query.CountAsync(ctx);

        // Apply ordering
        if (!string.IsNullOrWhiteSpace(request.OrderBy))
        {
            query = request.OrderDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, request.OrderBy))
                : query.OrderBy(e => EF.Property<object>(e, request.OrderBy));
        }
        else
        {
            // Default order
            query = query
                .OrderByDescending(p => p.Connection.CreatedAt)
                .ThenBy(p => p.Connection.FirstName);
        }

        var connections = await query.AsNoTracking()
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(p => new UserConnectionDTO
            {
                Id = p.Connection.AccountId,
                Name = $"{p.Connection.FirstName} {p.Connection.LastName}",
                Age = DateTime.Now.Year - p.Connection.BirthDay.Year -
                    (DateTime.Now.DayOfYear < p.Connection.BirthDay.DayOfYear ? 1 : 0),
                State = p.ConnectionType.MapToDto(),
                AvatarUrl = "" // TODO
            })
            .ToListAsync(ctx);

        return Result.Success(new UserConnectionResponse.Index
        {
            Connections = connections,
            TotalCount = totalCount
        });
    }
}
