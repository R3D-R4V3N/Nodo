using Ardalis.Result;
using Rise.Domain.Chats;
using Rise.Domain.Organizations;
using Rise.Domain.Users;
using System.Runtime.Intrinsics.X86;

namespace Rise.Domain.Tests.Users;
public class ApplicationUserTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        var accountId = "id";
        var firstName = TestData.ValidFirstName();
        var lastName = TestData.ValidLastName();
        var biography = TestData.ValidBiography();
        var avatarUrl = TestData.ValidAvatarUrl();
        var birthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-28));
        var userSettings = TestData.ValidUserSettings();
        var org = TestData.ValidOrganization();

        var user = new User()
        {
            AccountId = accountId,
            FirstName = firstName,
            LastName = lastName,
            Biography = biography,
            AvatarUrl = avatarUrl,
            BirthDay = birthDay,
            UserSettings = userSettings,
            Organization = org,
        };

        user.AccountId.ShouldBe(accountId);
        user.FirstName.ShouldBe(firstName);
        user.LastName.ShouldBe(lastName);
        user.Biography.ShouldBe(biography);
        user.AvatarUrl.ShouldBe(avatarUrl);
        user.BirthDay.ShouldBe(birthDay);
        user.UserSettings.ShouldBe(userSettings);
        user.Organization.ShouldBe(org);
    }

    [Fact]
    public void HasFriend_ShouldReturnTrue_WhenConnectionExists()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        user1.AddFriend(user2);
        user2.AddFriend(user1);

        user1.HasFriend(user2).ShouldBeTrue();
        user2.HasFriend(user1).ShouldBeTrue();
    }

    [Fact]
    public void HasFriend_ShouldReturnFalse_WhenNoConnectionExists()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        user1.HasFriend(user2).ShouldBeFalse();
        user2.HasFriend(user1).ShouldBeFalse();
    }

    [Fact]
    public void AddFriend_ShouldCreateFriendRequest_WhenNoConnectionExists()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        var result = user1.AddFriend(user2);

        result.IsSuccess.ShouldBeTrue();
        user1.FriendRequests.ShouldContain(x => x.Connection.Equals(user2));
        user2.FriendRequests.ShouldContain(x => x.Connection.Equals(user1));
    }

    [Fact]
    public void AddFriend_ShouldCreateFriendship_WhenRequestExistsFromOtherUser()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        user2.AddFriend(user1);

        var result = user1.AddFriend(user2);

        result.Status.ShouldBe(ResultStatus.Ok);
        user1.Friends.ShouldContain(x => x.Connection.Equals(user2));
        user2.Friends.ShouldContain(x => x.Connection.Equals(user1));
        user1.FriendRequests.ShouldBeEmpty();
        user2.FriendRequests.ShouldBeEmpty();
    }


    [Fact]
    public void AddFriend_ShouldReturnConflict_WhenAlreadyFriends()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);
        user1.AddFriend(user2);
        user2.AddFriend(user1);


        var result = user1.AddFriend(user2);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Gebruiker is al bevriend met {user2}"]);

        user1.Friends.ShouldContain(x => x.Connection.Equals(user2));
        user2.Friends.ShouldContain(x => x.Connection.Equals(user1));
    }

    [Fact]
    public void AddFriend_ShouldReturnConflict_WhenRequestExistsFromSameUser()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);
        user1.AddFriend(user2);

        var result = user1.AddFriend(user2);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Gebruiker heeft al een vriendschapsverzoek naar {user2} verstuurd"]);

        user1.FriendRequests.ShouldContain(x => x.Connection.Equals(user2));
        user2.FriendRequests.ShouldContain(x => x.Connection.Equals(user1));
    }

    [Fact]
    public void RemoveFriend_ShouldRemoveFromBothUsers()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);
        user1.AddFriend(user2);
        user2.AddFriend(user1); 

        var result = user1.RemoveFriend(user2);

        result.Status.ShouldBe(ResultStatus.Ok);
        user1.Friends.ShouldNotContain(x => x.Connection.Equals(user2));
        user2.Friends.ShouldNotContain(x => x.Connection.Equals(user1));
    }

    [Fact]
    public void RemoveFriendRequest_ShouldRemoveFromBothUsers()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);
        user1.AddFriend(user2);

        var result = user1.RemoveFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.Ok);
        user1.FriendRequests.ShouldNotContain(x => x.Connection.Equals(user2));
        user2.FriendRequests.ShouldNotContain(x => x.Connection.Equals(user1));
    }

    [Fact]
    public void AddChat_ShouldAddChatAndLinkBack()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        user1.AddFriend(user2);
        user2.AddFriend(user1);

        Chat chat = Chat.CreateChat(user1, user2);

        var newUser = TestData.ValidUser(3);
        user1.AddFriend(newUser);
        newUser.AddFriend(user1);

        var result = newUser.AddChat(user1, chat);

        result.IsSuccess.ShouldBeTrue();
        user2.Chats.ShouldContain(chat);
        chat.Users.ShouldContain(newUser);
        chat.Users.Count.ShouldBe(3);
    }

    [Theory]
    [InlineData(false)] 
    [InlineData(true)] 
    public void AddChat_SupervisorCanAdd_WithoutFriendship(bool isSelfOwner)
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        user1.AddFriend(user2);
        user2.AddFriend(user1);

        Chat chat = Chat.CreateChat(user1, user2);

        var supervisor = TestData.ValidSupervisor(3);
        BaseUser owner = isSelfOwner ? supervisor : user1;

        var result = supervisor.AddChat(owner, chat);

        result.IsSuccess.ShouldBeTrue();
        supervisor.Chats.ShouldContain(chat);
        chat.Users.ShouldContain(supervisor);
        chat.Users.Count.ShouldBe(3);
    }

    [Fact]
    public void AddChat_ShouldReturnConflict_WhenAlreadyInChat()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        user1.AddFriend(user2);
        user2.AddFriend(user1);

        Chat chat = Chat.CreateChat(user1, user2);

        var newUser = TestData.ValidUser(3);
        user1.AddFriend(newUser);
        newUser.AddFriend(user1);

        newUser.AddChat(user1, chat);

        var result = newUser.AddChat(user1, chat);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Gebruiker is al lid van chat {chat}"]);
    }

    [Fact]
    public void AddChat_ShouldReturnConflict_WhenNotFriend()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        user1.AddFriend(user2);
        user2.AddFriend(user1);

        Chat chat = Chat.CreateChat(user1, user2);

        var newUser = TestData.ValidUser(3);
        var result = newUser.AddChat(user1, chat);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"{user1} is niet bevriendt met {newUser}"]);
    }

    [Fact]
    public void AddChat_ShouldReturnConflict_AddUserFails()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);
        var user3 = TestData.ValidUser(3);

        user1.AddFriend(user2);
        user2.AddFriend(user1);

        Chat chat = Chat.CreateChat(user1, user2);

        var newUser = TestData.ValidUser(4);
        newUser.AddFriend(user3);
        user3.AddFriend(newUser);

        var result = newUser.AddChat(user3, chat);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldNotContain($"Gebruiker is al lid van chat {chat}");
        result.Errors.ShouldNotContain($"Chat eigenaar is niet bevriendt met {newUser}");
        newUser.Chats.ShouldNotContain(chat);
    }

    [Theory]
    [MemberData(nameof(RemoveChat_ShouldRemoveUserAndChatLink_MemberData))]
    public void RemoveChat_ShouldRemoveUserAndChatLink(User user1, User user2, User owner)
    {
        user1.AddFriend(user2);
        user2.AddFriend(user1);

        Chat chat = Chat.CreateChat(user1, user2);

        var result = user2.RemoveChat(owner, chat);

        result.IsSuccess.ShouldBeTrue();
        user2.Chats.ShouldNotContain(chat);
        chat.Users.Count.ShouldBe(1);
    }
    public static IEnumerable<object[]> RemoveChat_ShouldRemoveUserAndChatLink_MemberData()
    {
        yield return new object[] { TestData.ValidUser(1), TestData.ValidUser(2), TestData.ValidUser(1) };
        yield return new object[] { TestData.ValidUser(1), TestData.ValidUser(2), TestData.ValidUser(2) };
    }


    [Theory]
    [MemberData(nameof(RemoveChat_SupervisorCanRemove_WhenNotInChat_MemberData))]
    public void RemoveChat_SupervisorCanRemove_WhenNotInChat(User user1, User user2, User removedUser)
    {
        user1.AddFriend(user2);
        user2.AddFriend(user1);

        Chat chat = Chat.CreateChat(user1, user2);
        var supervisor = TestData.ValidSupervisor(3);

        var result = removedUser.RemoveChat(supervisor, chat);

        result.IsSuccess.ShouldBeTrue();
        removedUser.Chats.ShouldNotContain(chat);
        chat.Users.Count.ShouldBe(1);
    }
    public static IEnumerable<object[]> RemoveChat_SupervisorCanRemove_WhenNotInChat_MemberData()
    {
        yield return new object[] { TestData.ValidUser(1), TestData.ValidUser(2), TestData.ValidUser(1) };
        yield return new object[] { TestData.ValidUser(1), TestData.ValidUser(2), TestData.ValidUser(2) };
    }

    [Fact]
    public void RemoveChat_ShouldReturnConflict_WhenNotInChat()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        user1.AddFriend(user2);
        user2.AddFriend(user1);

        Chat chat = Chat.CreateChat(user1, user2);
        user2.RemoveChat(user2, chat);
        
        var result = user2.RemoveChat(user2, chat);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Gebruiker is geen lid van chat {chat}"]);
    }

    [Fact]
    public void RemoveChat_ShouldReturnConflict_RemoveUserFails()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);
        var user3 = TestData.ValidUser(3);

        user1.AddFriend(user2);
        user2.AddFriend(user1);

        Chat chat = Chat.CreateChat(user1, user2);

        var result = user2.RemoveChat(user3, chat);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldNotContain($"Gebruiker is geen lid van chat {chat}");
        user2.Chats.ShouldContain(chat);
    }
}
