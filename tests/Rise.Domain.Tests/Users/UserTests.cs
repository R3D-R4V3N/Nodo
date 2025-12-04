using Ardalis.Result;
using Rise.Domain.Chats;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Helper;
using Rise.Domain.Users;
using Rise.Domain.Users.Connections;
using Rise.Tests.Shared;
using Rise.Domain.Users.Sentiment;
using Rise.Domain.Users.Hobbys;

namespace Rise.Domain.Tests.Users;
public class UserTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        var accountId = "id";
        var firstName = DomainData.ValidFirstName();
        var lastName = DomainData.ValidLastName();
        var biography = DomainData.ValidBiography();
        var avatarUrl = DomainData.ValidAvatarUrl();
        var birthDay = DomainData.ValidBirthDay();
        var gender = GenderType.X;
        var userSettings = DomainData.ValidUserSettings();
        var orga = DomainData.ValidOrganization();
        var supervisor = DomainData.ValidSupervisor();

        var user = new User()
        {
            AccountId = accountId,
            FirstName = firstName,
            LastName = lastName,
            Biography = biography,
            AvatarUrl = avatarUrl,
            BirthDay = birthDay,
            Gender = gender,
            UserSettings = userSettings,
            Organization = orga,
            Supervisor = supervisor
        };

        user.AccountId.ShouldBe(accountId);
        user.FirstName.ShouldBe(firstName);
        user.LastName.ShouldBe(lastName);
        user.Biography.ShouldBe(biography);
        user.AvatarUrl.ShouldBe(avatarUrl);
        user.BirthDay.ShouldBe(birthDay);
        user.Gender.ShouldBe(gender);
        user.UserSettings.ShouldBe(userSettings);
        user.Organization.ShouldBe(orga);
        user.Supervisor.ShouldBe(supervisor);
    }

    [Theory]
    [InlineData(UserConnectionType.None, 0)]
    [InlineData(UserConnectionType.RequestIncoming, 0)]
    [InlineData(UserConnectionType.RequestOutgoing, 0)]
    [InlineData(UserConnectionType.Friend, 1)]
    [InlineData(UserConnectionType.Blocked, 0)]
    public void Friends_ReturnsFriends(UserConnectionType type, int count)
    {
        var user = DomainData
            .ValidUser();

        user = user
            .WithConnections(
                user.CreateConnectionWith(DomainData.ValidUser(), type)
            );

        user.Friends.Count().ShouldBe(count);
    }

    [Theory]
    [InlineData(UserConnectionType.None, 0)]
    [InlineData(UserConnectionType.RequestIncoming, 1)]
    [InlineData(UserConnectionType.RequestOutgoing, 1)]
    [InlineData(UserConnectionType.Friend, 0)]
    [InlineData(UserConnectionType.Blocked, 0)]
    public void FriendRequests_ReturnsFriendRequests(UserConnectionType type, int count)
    {
        var user = DomainData
            .ValidUser();

        user = user
            .WithConnections(
                user.CreateConnectionWith(DomainData.ValidUser(), type)
            );

        user.FriendRequests.Count().ShouldBe(count);
    }

    [Theory]
    [InlineData(UserConnectionType.None, 0)]
    [InlineData(UserConnectionType.RequestIncoming, 0)]
    [InlineData(UserConnectionType.RequestOutgoing, 0)]
    [InlineData(UserConnectionType.Friend, 0)]
    [InlineData(UserConnectionType.Blocked, 1)]
    public void BlockedUsers_ReturnsBlockedUsers(UserConnectionType type, int count)
    {
        var user = DomainData
            .ValidUser();

        user = user
            .WithConnections(
                user.CreateConnectionWith(DomainData.ValidUser(), type)
            );

        user.BlockedUsers.Count().ShouldBe(count);
    }

    [Fact]
    public void HasFriend_ShouldReturnTrue_WhenConnectionExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1 = user1
            .WithConnections(
                user1.CreateConnectionWith(user2, UserConnectionType.Friend)
            );

        user2 = user2
            .WithConnections(
                user2.CreateConnectionWith(user1, UserConnectionType.Friend)
            );

        user1.IsFriend(user2).ShouldBeTrue();
        user2.IsFriend(user1).ShouldBeTrue();
    }

    [Fact]
    public void IsFriend_ShouldReturnFalse_WhenNoFriendshipExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.IsFriend(user2).ShouldBeFalse();
        user2.IsFriend(user1).ShouldBeFalse();
    }

    [Fact]
    public void IsBlocked_ShouldReturnTrue_WhenBlockExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1 = user1
            .WithConnections(
                user1.CreateConnectionWith(user2, UserConnectionType.Blocked)
            );

        user1.IsBlocked(user2).ShouldBeTrue();
    }

    [Fact]
    public void IsBlocked_ShouldReturnFalse_WhenNoBlockExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.IsBlocked(user2).ShouldBeFalse();
        user2.IsBlocked(user1).ShouldBeFalse();
    }

    [Fact]
    public void HasFriendRequest_ShouldReturnTrue_WhenIncomingRequestExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1 = user1
            .WithConnections(
                user1.CreateConnectionWith(user2, UserConnectionType.RequestIncoming)
            );

        user2 = user2
            .WithConnections(
                user2.CreateConnectionWith(user1, UserConnectionType.RequestIncoming)
            );

        user1.HasFriendRequest(user2).ShouldBeTrue();
        user2.HasFriendRequest(user1).ShouldBeTrue();
    }

    [Fact]
    public void HasFriendRequest_ShouldReturnTrue_WhenOutgoingRequestExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1 = user1
            .WithConnections(
                user1.CreateConnectionWith(user2, UserConnectionType.RequestOutgoing)
            );

        user2 = user2
            .WithConnections(
                user2.CreateConnectionWith(user1, UserConnectionType.RequestOutgoing)
            );

        user1.HasFriendRequest(user2).ShouldBeTrue();
        user2.HasFriendRequest(user1).ShouldBeTrue();
    }

    [Fact]
    public void HasFriendRequest_ShouldReturnFalse_WhenNoRequestExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.IsFriend(user2).ShouldBeFalse();
        user2.IsFriend(user1).ShouldBeFalse();
    }

    [Fact]
    public void SendFriendRequest_ShouldCreateFriendRequest_WhenNoConnectionExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        var result = user1.SendFriendRequest(user2);

        result.IsSuccess.ShouldBeTrue();
        user1.FriendRequests.ShouldContain(x => x.To.Equals(user2) && x.ConnectionType.Equals(UserConnectionType.RequestOutgoing));
        user2.FriendRequests.ShouldContain(x => x.To.Equals(user1) && x.ConnectionType.Equals(UserConnectionType.RequestIncoming));
    }

    [Fact]
    public void SendFriendRequest_ShouldReturnConflict_WhenTargetHasCurrentBlocked()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user2 = user2.WithConnections(
            user2.CreateConnectionWith(user1, UserConnectionType.Blocked)
        );

        var result = user1.SendFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"{user2} heeft je geblokkeerd"]);
    }

    [Theory]
    [InlineData(UserConnectionType.Friend)]
    [InlineData(UserConnectionType.RequestIncoming)]
    [InlineData(UserConnectionType.RequestOutgoing)]
    [InlineData(UserConnectionType.Blocked)]
    public void SendFriendRequest_ShouldReturnConflict_WhenConnectionAlreadyExists(UserConnectionType type)
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1 = user1.WithConnections(
            user1.CreateConnectionWith(user2, type)
        );

        var result = user1.SendFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.Count().ShouldBe(1);
    }

    [Fact]
    public void AcceptFriendRequest_ShouldCreateFriendship_WhenIncomingConnectionExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        var result = user2.AcceptFriendRequest(user1);

        result.IsSuccess.ShouldBeTrue();

        user1.Friends.ShouldContain(x => x.To.Equals(user2));
        user1.FriendRequests.ShouldBeEmpty();

        user2.Friends.ShouldContain(x => x.To.Equals(user1));
        user2.FriendRequests.ShouldBeEmpty();
    }

    [Fact]
    public void AcceptFriendRequest_ShouldReturnConflict_WhenNoConnectionExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        var result = user1.AcceptFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.NotFound);
        result.Errors.ShouldBe([$"Er is geen veroek van {user2} om te accepteren"]);

        user1.FriendRequests.ShouldBeEmpty();
        user1.Friends.ShouldBeEmpty();
        user2.FriendRequests.ShouldBeEmpty();
        user2.Friends.ShouldBeEmpty();
    }

    [Fact]
    public void AcceptFriendRequest_ShouldReturnConflict_WhenAlreadyFriends()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        var result = user1.AcceptFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"{user1} is al bevriend met {user2}"]);

        user1.Friends.Count().ShouldBe(1);
        user1.FriendRequests.ShouldBeEmpty();

        user2.Friends.Count().ShouldBe(1);
        user2.FriendRequests.ShouldBeEmpty();
    }

    [Fact]
    public void AcceptFriendRequest_ShouldReturnConflict_WhenNoIncoming()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);

        var result = user1.AcceptFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Er is geen verzoek van {user2} om te accepteren"]);

        user1.Friends.ShouldBeEmpty();
        user1.FriendRequests.Count().ShouldBe(1);

        user2.Friends.ShouldBeEmpty();
        user2.FriendRequests.Count().ShouldBe(1); ;
    }

    [Fact]
    public void RemoveFriend_ShouldRemoveFromBothUsers()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        user1.Friends.ShouldNotBeEmpty();
        user2.Friends.ShouldNotBeEmpty();

        var result = user1.RemoveFriend(user2);

        result.Status.ShouldBe(ResultStatus.Ok);
        user1.Friends.ShouldBeEmpty();
        user2.Friends.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveFriend_ShouldNotFound_WhenNoConnectionExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        var result = user1.RemoveFriend(user2);

        result.Status.ShouldBe(ResultStatus.NotFound);
        result.Errors.ShouldBe([$"Je bent niet bevriend met {user2}"]);
    }

    [Fact]
    public void RemoveFriend_ShouldNotFound_WhenNoFriendshipExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);

        var result = user1.RemoveFriend(user2);

        result.Status.ShouldBe(ResultStatus.NotFound);
        result.Errors.ShouldBe([$"Je bent niet bevriend met {user2}"]);
    }

    [Fact]
    public void CancelFriendRequest_ShouldRemoveFromBothUsers()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);

        var result = user1.CancelFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.Ok);
        user1.FriendRequests.ShouldBeEmpty();
        user2.FriendRequests.ShouldBeEmpty();
    }

    [Fact]
    public void CancelFriendRequest_NotFound_NoConnectionToCancel()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        var result = user1.CancelFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.NotFound);
        result.Errors.ShouldBe([$"Er is geen verzoek naar {user2} om te annuleren."]);
        user1.FriendRequests.ShouldBeEmpty();
        user2.FriendRequests.ShouldBeEmpty();
    }

    [Fact]
    public void CancelFriendRequest_ShouldConflict_UsersAreFriends()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1); 

        var result = user1.CancelFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"{user1} is al bevriend met {user2} en kan geen verzoek annuleren."]);
        user1.Friends.ShouldNotBeEmpty();
        user2.Friends.ShouldNotBeEmpty();
    }

    [Fact]
    public void CancelFriendRequest_ShouldConflict_ConnectionButNoOutgoingConnection()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user2.SendFriendRequest(user1);

        var result = user1.CancelFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Er is geen uitgaand verzoek van {user1} naar {user2} om te annuleren."]);
        user1.FriendRequests.ShouldNotBeEmpty();
        user2.FriendRequests.ShouldNotBeEmpty();
    }

    [Fact]
    public void RejectFriendRequest_ShouldRemoveFromBothUsers()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);

        var result = user2.RejectFriendRequest(user1);

        result.Status.ShouldBe(ResultStatus.Ok);
        user1.FriendRequests.ShouldBeEmpty();
        user2.FriendRequests.ShouldBeEmpty();
    }

    [Fact]
    public void RejectFriendRequest_NotFound_NoConnectionToReject()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        var result = user1.RejectFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.NotFound);
        result.Errors.ShouldBe([$"Er is geen verzoek van {user2} om te weigeren"]);
        user1.FriendRequests.ShouldBeEmpty();
        user2.FriendRequests.ShouldBeEmpty();
    }

    [Fact]
    public void RejectFriendRequest_ShouldConflict_UsersAreFriends()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        var result = user1.RejectFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"{user1} is al bevriend met {user2} en kan geen verzoek weigeren."]);
        user1.Friends.ShouldNotBeEmpty();
        user2.Friends.ShouldNotBeEmpty();
    }

    [Fact]
    public void RejectFriendRequest_ShouldConflict_ConnectionButNoIncomingConnection()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);

        var result = user1.RejectFriendRequest(user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Er is geen uitgaand verzoek van {user1} naar {user2} om te weigeren."]);
        user1.FriendRequests.ShouldNotBeEmpty();
        user2.FriendRequests.ShouldNotBeEmpty();
    }

    [Fact]
    public void AddChat_ShouldAddChatAndLinkBack()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateGroupChat(user1, user2);

        var newUser = DomainData.ValidUser();
        user1.SendFriendRequest(newUser);
        newUser.AcceptFriendRequest(user1);

        var result = newUser.AddChat(user1, chat);

        result.IsSuccess.ShouldBeTrue();
        user2.Chats.ShouldContain(chat);
        chat.Users.ShouldContain(newUser);
        chat.Users.Count.ShouldBe(3);
    }

    [Fact]
    public void AddChat_ShouldReturnConflict_WhenPrivateChat()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreatePrivateChat(user1, user2);

        var newUser = DomainData.ValidUser();
        user1.SendFriendRequest(newUser);
        newUser.AcceptFriendRequest(user1);

        var result = newUser.AddChat(user1, chat);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Er kunnen geen gebruikers toegevoegd worden aan een private chat"]);
    }

    [Fact]
    public void AddChat_ShouldAdd_WhenSupervisorIsAddedToPrivate()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreatePrivateChat(user1, user2);

        var supervisor = DomainData.ValidSupervisor();

        var result = supervisor.AddChat(user1, chat);

        result.IsSuccess.ShouldBeTrue();
        user2.Chats.ShouldContain(chat);
        chat.Users.ShouldContain(supervisor);
        chat.Users.Count.ShouldBe(3);
    }

    [Fact]
    public void AddChat_ShouldReturnConflict_WhenAlreadyInChat()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateGroupChat(user1, user2);

        var newUser = DomainData.ValidUser();
        user1.SendFriendRequest(newUser);
        newUser.AcceptFriendRequest(user1);

        newUser.AddChat(user1, chat);

        var result = newUser.AddChat(user1, chat);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Gebruiker is al lid van chat {chat}"]);
    }

    [Fact]
    public void AddChat_ShouldReturnConflict_WhenNotFriend()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.AcceptFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreatePrivateChat(user1, user2);

        var newUser = DomainData.ValidUser();
        var result = newUser.AddChat(user1, chat);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"{user1} is niet bevriendt met {newUser}"]);
    }

    [Fact]
    public void AddChat_ShouldReturnConflict_AddUserFails()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();
        var user3 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreatePrivateChat(user1, user2);

        var newUser = DomainData.ValidUser();
        newUser.SendFriendRequest(user3);
        user3.AcceptFriendRequest(newUser);

        var result = newUser.AddChat(user3, chat);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldNotContain($"Gebruiker is al lid van chat {chat}");
        result.Errors.ShouldNotContain($"Chat eigenaar is niet bevriendt met {newUser}");
        newUser.Chats.ShouldNotContain(chat);
    }

    [Theory]
    [MemberData(nameof(RemoveChat_ShouldRemoveUserAndChatLink_MemberData))]
    public void RemoveChat_ShouldRemoveUserAndChatLink(User user1, User user2, bool ownerIsUser1)
    {
        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateGroupChat(user1, user2);
        var owner = ownerIsUser1 ? user1 : user2;

        var result = user2.RemoveChat(owner, chat);

        result.IsSuccess.ShouldBeTrue();
        user2.Chats.ShouldNotContain(chat);
        chat.Users.Count.ShouldBe(1);
    }
    public static IEnumerable<object[]> RemoveChat_ShouldRemoveUserAndChatLink_MemberData()
    {
        yield return new object[] { DomainData.ValidUser(), DomainData.ValidUser(), true };
        yield return new object[] { DomainData.ValidUser(), DomainData.ValidUser(), false };
    }

    [Theory]
    [MemberData(nameof(RemoveChat_ShouldConflict_WhenPrivateChat_MemberData))]
    public void RemoveChat_ShouldConflict_WhenPrivateChat(User user1, User user2, User owner)
    {
        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreatePrivateChat(user1, user2);

        var result = user2.RemoveChat(owner, chat);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldNotContain($"Gebruiker is geen lid van chat {chat}");
    }
    public static IEnumerable<object[]> RemoveChat_ShouldConflict_WhenPrivateChat_MemberData()
    {
        yield return new object[] { DomainData.ValidUser(), DomainData.ValidUser(), DomainData.ValidUser() };
        yield return new object[] { DomainData.ValidUser(), DomainData.ValidUser(), DomainData.ValidUser() };
    }

    [Fact]
    public void RemoveChat_ShouldReturnConflict_WhenNotInChat()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateGroupChat(user1, user2);
        user2.RemoveChat(user2, chat);
        
        var result = user2.RemoveChat(user2, chat);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Gebruiker is geen lid van chat {chat}"]);
    }

    [Fact]
    public void RemoveChat_ShouldReturnConflict_RemoveUserFails()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();
        var user3 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreatePrivateChat(user1, user2);

        var result = user2.RemoveChat(user3, chat);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldNotContain($"Gebruiker is geen lid van chat {chat}");
        user2.Chats.ShouldContain(chat);
    }

    [Fact]
    public void UpdateSentiments_ShouldUpdate()
    {
        var user = DomainData.ValidUser();
        IEnumerable<UserSentiment> sentiments = 
        [
            new UserSentiment()
            { 
                Type = SentimentType.Like,
                Category = SentimentCategoryType.ActionMovies,
            }.WithId(1),
            new UserSentiment()
            {
                Type = SentimentType.Dislike,
                Category = SentimentCategoryType.AmusementParks,
            }.WithId(2),
        ];

        var result = user.UpdateSentiments(sentiments);

        result.IsSuccess.ShouldBeTrue();
        user.Sentiments.ShouldBe(sentiments);
    }

    [Fact]
    public void UpdateSentiments_ShouldUpdate_WhenEmpty()
    {
        var user = DomainData.ValidUser();
        IEnumerable<UserSentiment> sentiments = [];

        var result = user.UpdateSentiments(sentiments);

        result.IsSuccess.ShouldBeTrue();
        user.Sentiments.ShouldBeEmpty();
    }

    [Fact]
    public void UpdateSentiments_ShouldUpdate_WhenListNull()
    {
        var user = DomainData.ValidUser();

        var result = user.UpdateSentiments(null!);

        result.IsSuccess.ShouldBeTrue();
        user.Sentiments.ShouldBeEmpty();
    }

    [Fact]
    public void UpdateSentiments_ShouldConflict_WhenItemNull()
    {
        IEnumerable<UserSentiment> sentiments =
        [
            new UserSentiment()
            {
                Type = SentimentType.Like,
                Category = SentimentCategoryType.ActionMovies,
            }.WithId(1),
        ];

        var user = DomainData
            .ValidUser()
            .WithSentiments(sentiments);

        IEnumerable<UserSentiment> updateSentiments =
        [
            null!
        ];

        var result = user.UpdateSentiments(updateSentiments);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Gevoelens bevat een null value"]);
        user.Sentiments.ShouldBe(sentiments);
    }

    [Fact]
    public void UpdateSentiments_ShouldUpdate_WhenDuplicate()
    {
        var user = DomainData
            .ValidUser();

        IEnumerable<UserSentiment> sentiments =
        [
            new UserSentiment()
            {
                Type = SentimentType.Like,
                Category = SentimentCategoryType.ActionMovies,
            }.WithId(1),
            new UserSentiment()
            {
                Type = SentimentType.Like,
                Category = SentimentCategoryType.ActionMovies,
            }.WithId(1),
        ];

        IEnumerable<UserSentiment> resultSentiments = sentiments.Distinct();

        var result = user.UpdateSentiments(sentiments);

        result.IsSuccess.ShouldBeTrue();
        user.Sentiments.ShouldBe(resultSentiments);
    }

    [Theory]
    [InlineData(SentimentType.Like)]
    [InlineData(SentimentType.Dislike)]
    public void UpdateSentiments_ShouldConflict_WhenTypeLimitExceeded(SentimentType type)
    {
        IEnumerable<UserSentiment> sentiments =
        [
            new UserSentiment()
            {
                Type = SentimentType.Like,
                Category = SentimentCategoryType.ActionMovies,
            }.WithId(1),
        ];

        var user = DomainData
            .ValidUser()
            .WithSentiments(sentiments);

        List<UserSentiment> updateSentiments = [];

        for (int i = 1; i <= User.MAX_SENTIMENTS_PER_TYPE + 1; i++)
        {
            updateSentiments.Add(
                new UserSentiment()
                {
                    Type = type,
                    Category = SentimentCategoryType.ActionMovies,
                }.WithId(i)
            );
        }

        var result = user.UpdateSentiments(updateSentiments);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Mag maximaal {User.MAX_SENTIMENTS_PER_TYPE} van een gevoelens type hebben, {type} overschreed dit"]);
        user.Sentiments.ShouldBe(sentiments);
    }

    [Fact]
    public void UpdateSentiments_ShouldConflict_WhenConflictingSentiments()
    {
        IEnumerable<UserSentiment> sentiments =
        [
            new UserSentiment()
            {
                Type = SentimentType.Like,
                Category = SentimentCategoryType.ActionMovies,
            }.WithId(1),
        ];

        var user = DomainData
            .ValidUser()
            .WithSentiments(sentiments);

        IEnumerable<UserSentiment> updateSentiments =
        [
            new UserSentiment()
            {
                Type = SentimentType.Like,
                Category = SentimentCategoryType.ActionMovies,
            }.WithId(1),
            new UserSentiment()
            {
                Type = SentimentType.Dislike,
                Category = SentimentCategoryType.ActionMovies,
            }.WithId(2),
        ];

        var result = user.UpdateSentiments(updateSentiments);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe(["Bevat duplicaat category in een andere gevoel"]);
        user.Sentiments.ShouldBe(sentiments);
    }

    [Fact]
    public void UpdateHobbies_ShouldUpdate()
    {
        var user = DomainData.ValidUser();
        IEnumerable<UserHobby> hobbies =
        [
            new UserHobby()
            {
                Hobby = HobbyType.Swimming,
            }.WithId(1),
        ];

        var result = user.UpdateHobbies(hobbies);

        result.IsSuccess.ShouldBeTrue();
        user.Hobbies.ShouldBe(hobbies);
    }

    [Fact]
    public void UpdateHobbies_ShouldConflict_WhenLimitExceeded()
    {
        IEnumerable<UserHobby> hobbies =
        [
            new UserHobby()
            {
                Hobby = HobbyType.Swimming,
            }.WithId(1),
        ];

        var user = DomainData
            .ValidUser()
            .WithHobbies(hobbies);

        List<UserHobby> updateHobbies = [];

        for (int i = 1; i <= User.MAX_HOBBIES + 1; i++)
        {
            updateHobbies.Add(
                new UserHobby()
                {
                    Hobby = HobbyType.Swimming,
                }.WithId(i)
            );
        }

        var result = user.UpdateHobbies(updateHobbies);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Mag maximaal {User.MAX_HOBBIES} hobbies hebben."]);
        user.Hobbies.ShouldBe(hobbies);
    }

    [Fact]
    public void UpdateHobbies_ShouldConflict_WhenItemNull()
    {
        IEnumerable<UserHobby> hobbies =
        [
            new UserHobby()
            {
                Hobby = HobbyType.Swimming,
            }.WithId(1),
        ];

        var user = DomainData
            .ValidUser()
            .WithHobbies(hobbies);

        List<UserHobby> updateHobbies = 
        [
            null!
        ];

        var result = user.UpdateHobbies(updateHobbies);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Hobbies bevat een null waarde"]);
        user.Hobbies.ShouldBe(hobbies);
    }
}
