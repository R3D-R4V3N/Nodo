using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Settings;
using Rise.Domain.Users.Settings.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Domain.Tests;

public static class TestData
{
    public static DefaultSentence ValidDefaultSentence(int idx = 0)
        => DefaultSentence.Create($"Valid Sentence {idx}.");

    public static FontSize ValidFontSize()
        => FontSize.Create(new Random().Next(FontSize.MIN_FONT_SIZE, FontSize.MAX_FONT_SIZE));

    public static UserSetting ValidUserSettings() =>
        new UserSetting
        {
            FontSize = ValidFontSize()
        };

    public static FirstName ValidFirstName() => FirstName.Create($"John");
    public static LastName ValidLastName() => LastName.Create($"Doe");
    public static Biography ValidBiography() => Biography.Create($"Dit is een bio.");
    public static AvatarUrl ValidAvatarUrl() => AvatarUrl.Create($"Dit is een img.");
    public static User ValidUser(int id) =>
        (User) new User()
        {
            AccountId = "valid-id-" + id,
            FirstName = ValidFirstName(),
            LastName = ValidLastName(),
            Biography = ValidBiography(),
            AvatarUrl = ValidAvatarUrl(),
            BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-28)),
            UserSettings = ValidUserSettings()
        }.WithId(id);

    public static Supervisor ValidSupervisor(int id) =>
        (Supervisor) new Supervisor()
        {
            AccountId = "valid-id-" + id,
            FirstName = ValidFirstName(),
            LastName = ValidLastName(),
            Biography = ValidBiography(),
            AvatarUrl = ValidAvatarUrl(),
            BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-40)),
            UserSettings = ValidUserSettings()
        }.WithId(id);

    private static BaseUser WithId(this BaseUser user, int id)
    {
        typeof(BaseUser)
            .GetProperty(nameof(BaseUser.Id))!
            .SetValue(user, id);

        return user;
    }

    public static Chat ValidChat()
    {
        var user1 = ValidUser(-1);
        var user2 = ValidUser(-2);

        user1.AddFriend(user2);
        user2.AddFriend(user1);

        return Chat.CreateChat(user1, user2);
    }
}
