using Ardalis.Result;
using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;

namespace Rise.Domain.Tests.Users;
public class SupervisorTests
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
        var gender = GenderType.X;
        var userSettings = TestData.ValidUserSettings();

        var user = new Supervisor()
        {
            AccountId = accountId,
            FirstName = firstName,
            LastName = lastName,
            Biography = biography,
            AvatarUrl = avatarUrl,
            BirthDay = birthDay,
            Gender = gender,
            UserSettings = userSettings
        };

        user.AccountId.ShouldBe(accountId);
        user.FirstName.ShouldBe(firstName);
        user.LastName.ShouldBe(lastName);
        user.Biography.ShouldBe(biography);
        user.AvatarUrl.ShouldBe(avatarUrl);
        user.BirthDay.ShouldBe(birthDay);
        user.Gender.ShouldBe(gender);
        user.UserSettings.ShouldBe(userSettings);
    }

    [Theory]
    [InlineData(false)] 
    [InlineData(true)] 
    public void AddChat_SupervisorCanAdd_WithoutFriendship(bool isSelfOwner)
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

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
    public void RemoveChat_SupervisorCanRemove_WhenNotInChat()
    {
        User user1 = TestData.ValidUser(1);
        User user2 = TestData.ValidUser(2);

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateChat(user1, user2);
        var supervisor = TestData.ValidSupervisor(3);

        var result = user1.RemoveChat(supervisor, chat);

        result.IsSuccess.ShouldBeTrue();
        user1.Chats.ShouldNotContain(chat);
        chat.Users.Count.ShouldBe(1);
    }
}
