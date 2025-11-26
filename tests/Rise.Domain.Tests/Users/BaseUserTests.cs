using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Users;
using Rise.Tests.Shared;

namespace Rise.Domain.Tests.Users;
public class BaseUserTests
{
    private class DummyUser : BaseUser { }
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
