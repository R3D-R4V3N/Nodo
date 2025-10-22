<<<<<<< HEAD
﻿using Microsoft.EntityFrameworkCore;
=======
using Microsoft.EntityFrameworkCore;
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.UserConnections.Mapper;
using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.UserConnections;
<<<<<<< HEAD
=======
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
>>>>>>> codex/add-alert-message-for-supervisor-monitoring

namespace Rise.Services.UserConnections;
/// <summary>
/// Service for user connections. Note the use of <see cref="ISessionContextProvider"/> to get the current user in this layer of the application.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="sessionContextProvider"></param>
public class UserConnectionService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider) : IUserConnectionService
{
<<<<<<< HEAD
    public async Task<Result<UserConnectionResponse.Index>> GetFriendIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
=======
    public async Task<Result<UserConnectionResponse.GetFriends>> GetFriendIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
    {
        var userId = sessionContextProvider.User!.GetUserId();

        var loggedInUser = await dbContext.ApplicationUsers
            .SingleOrDefaultAsync(x => x.AccountId == sessionContextProvider.User!.GetUserId(), ctx);

        if (loggedInUser is null)
            return Result.Unauthorized("You are not authorized to fetch user connections.");

<<<<<<< HEAD
        var connectionQuery = dbContext.ApplicationUsers
=======
        var query = dbContext.ApplicationUsers
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
            .Where(u => u.AccountId == userId)
            .SelectMany(u => EF.Property<IEnumerable<UserConnection>>(u, "_connections"))
            .Where(c =>
                c.ConnectionType.Equals(UserConnectionType.Friend)
                || c.ConnectionType.Equals(UserConnectionType.RequestIncoming)
                || c.ConnectionType.Equals(UserConnectionType.RequestOutgoing));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
<<<<<<< HEAD
            var searchTerm = request.SearchTerm.Trim();
            connectionQuery = connectionQuery.Where(p =>
                p.Connection.FirstName.Contains(searchTerm)
                || p.Connection.LastName.Contains(searchTerm));
        }

        var connectionItems = await connectionQuery
            .Select(p => new
            {
                p.Connection.AccountId,
                p.Connection.FirstName,
                p.Connection.LastName,
                p.Connection.BirthDay,
                p.Connection.CreatedAt,
                p.Connection.UserType,
                p.ConnectionType
            })
            .ToListAsync(ctx);

        var connections = connectionItems
            .Select(p => new UserConnectionDTO
            {
                Id = p.AccountId,
                Name = $"{p.FirstName} {p.LastName}",
                Age = CalculateAge(p.BirthDay),
                State = p.ConnectionType.MapToDto(),
                AvatarUrl = ""
            })
            .ToList();

        var connectedAccountIds = connectionItems
            .Select(p => p.AccountId)
            .ToHashSet(StringComparer.Ordinal);

        var potentialQuery = dbContext.ApplicationUsers
            .Where(u =>
                u.UserType == UserType.Regular
                && u.AccountId != userId
                && !connectedAccountIds.Contains(u.AccountId));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();
            potentialQuery = potentialQuery.Where(u =>
                u.FirstName.Contains(searchTerm)
                || u.LastName.Contains(searchTerm));
        }

        var potentialConnections = await potentialQuery
            .Select(u => new UserConnectionDTO
            {
                Id = u.AccountId,
                Name = $"{u.FirstName} {u.LastName}",
                Age = CalculateAge(u.BirthDay),
                State = UserConnectionTypeDto.AddFriends,
                AvatarUrl = ""
            })
            .ToListAsync(ctx);

        var allConnections = connections
            .Concat(potentialConnections)
            .ToList();

        if (!string.IsNullOrWhiteSpace(request.OrderBy))
        {
            var propertyInfo = typeof(UserConnectionDTO).GetProperty(request.OrderBy);
            if (propertyInfo is not null)
            {
                allConnections = request.OrderDescending
                    ? allConnections.OrderByDescending(c => propertyInfo.GetValue(c, null)).ToList()
                    : allConnections.OrderBy(c => propertyInfo.GetValue(c, null)).ToList();
            }
        }

        var pagedConnections = allConnections
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        return Result.Success(new UserConnectionResponse.Index
        {
            Connections = pagedConnections,
            TotalCount = allConnections.Count
=======
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
            .Include(x => x.Connection)
            .ToListAsync(ctx);

        return Result.Success(new UserConnectionResponse.GetFriends
        {
            Connections = connections.Select(UserConnectionMapper.ToIndexUserConnectionDto),
            TotalCount = totalCount
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
        });
    }

    private static int CalculateAge(DateOnly birthDay)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - birthDay.Year;

        if (today < birthDay.AddYears(age))
        {
            age--;
        }

        return age;
    }
<<<<<<< HEAD
=======
    
    public async Task<Result<string>> AddFriendAsync(string targetAccountId, CancellationToken ctx = default)
    {
        var currentUserId = sessionContextProvider.User!.GetUserId();

        // Huidige gebruiker ophalen
        var currentUser = await dbContext.ApplicationUsers
            .Include(u => EF.Property<IEnumerable<UserConnection>>(u, "_connections"))
            .SingleOrDefaultAsync(u => u.AccountId == currentUserId, ctx);

        // Doelgebruiker ophalen
        var targetUser = await dbContext.ApplicationUsers
            .Include(u => EF.Property<IEnumerable<UserConnection>>(u, "_connections"))
            .SingleOrDefaultAsync(u => u.AccountId == targetAccountId, ctx);

        if (currentUser is null || targetUser is null)
            return Result.NotFound("User not found.");

        // Domeinlogica hergebruiken
        var result = currentUser.AddFriend(targetUser);

        if (!result.IsSuccess)
            return result; // bijv. conflict of invalid state

        await dbContext.SaveChangesAsync(ctx);
        return result;
    }
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
}
