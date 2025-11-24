using Ardalis.Result;
using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;

namespace Rise.Domain.Tests.Users;
public class BaseUserTests
{
    private class DummyUser : BaseUser { }
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

        var user = new DummyUser()
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
}
