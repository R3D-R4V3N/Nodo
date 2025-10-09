using Ardalis.Result;
using Rise.Domain.Users;
using Shouldly;
using Xunit;

namespace Rise.Domain.Tests.Users;

public class ApplicationUserFriendshipShould
{
    private static ApplicationUser CreateUser(string accountId, string firstName = "Jane", string lastName = "Doe")
    {
        var user = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            Biography = "Test user",
            UserType = UserType.Technician
        };

        typeof(ApplicationUser)
            .GetProperty(nameof(ApplicationUser.AccountId))!
            .SetValue(user, accountId);

        return user;
    }

    [Fact]
    public void NotAllowAddingNullFriend()
    {
        var user = CreateUser("user-1");

        Result result = user.AddFriend(null!);

        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.ErrorMessage.Contains("Friend is required"));
    }

    [Fact]
    public void NotAllowSelfFriendship()
    {
        var user = CreateUser("user-1");

        Result result = user.AddFriend(user);

        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.ErrorMessage.Contains("cannot add themselves"));
    }

    [Fact]
    public void CreateBidirectionalFriendship()
    {
        var user = CreateUser("user-1");
        var friend = CreateUser("user-2", firstName: "John");

        Result result = user.AddFriend(friend);

        result.IsSuccess.ShouldBeTrue();
        user.Friends.ShouldContain(friend);
        friend.Friends.ShouldContain(user);
    }

    [Fact]
    public void RemoveFriendshipFromBothUsers()
    {
        var user = CreateUser("user-1");
        var friend = CreateUser("user-2");
        user.AddFriend(friend);

        Result result = user.RemoveFriend(friend);

        result.IsSuccess.ShouldBeTrue();
        user.Friends.ShouldNotContain(friend);
        friend.Friends.ShouldNotContain(user);
    }

    [Fact]
    public void ConflictWhenRemovingNonExistingFriendship()
    {
        var user = CreateUser("user-1");
        var stranger = CreateUser("user-2");

        Result result = user.RemoveFriend(stranger);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("wasn't friends"));
    }
}
