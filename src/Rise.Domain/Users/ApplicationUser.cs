using Ardalis.GuardClauses;
using Ardalis.Result;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Rise.Domain.Common;

namespace Rise.Domain.Users;

public class ApplicationUser : Entity
{
    /// <summary>
    /// Link to the <see cref="IdentityUser"/> account so chatprofielen gekoppeld blijven aan hun login.
    /// </summary>
    public string AccountId { get; private set; }

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Biography { get; set; }
    public required UserType UserType { get; set; }

    private readonly HashSet<ApplicationUser> friends = [];
    public IReadOnlyCollection<ApplicationUser> Friends => friends;

    private readonly HashSet<ApplicationUser> friendRequests = [];
    public IReadOnlyCollection<ApplicationUser> FriendRequests => friendRequests;

    public ApplicationUser()
    {
        AccountId = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Biography = string.Empty;
    }

    [SetsRequiredMembers]
    public ApplicationUser(string accountId, string firstName, string lastName, string biography, UserType userType)
    {
        AccountId = Guard.Against.NullOrWhiteSpace(accountId);
        FirstName = Guard.Against.NullOrWhiteSpace(firstName);
        LastName = Guard.Against.NullOrWhiteSpace(lastName);
        Biography = Guard.Against.NullOrWhiteSpace(biography);
        UserType = userType;
    }

    public Result AddFriend(ApplicationUser friend)
    {
        if (!friendRequests.Contains(friend))
            return Result.Conflict($"Can't add {friend} without a request first");

        bool isAdded = friends.Add(friend);
        if (!isAdded)
            return Result.Conflict($"User is already friends with {friend}");

        friendRequests.Remove(friend);
        friend.friends.Add(this);

        return Result.Success();
    }

    public Result RemoveFriend(ApplicationUser friend)
    {
        bool isRemoved = friends.Remove(friend);
        if (!isRemoved)
            return Result.Conflict($"User wasn't friends with {friend}");

        friend.friends.Remove(this);

        return Result.Success();
    }
}
