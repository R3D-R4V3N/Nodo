using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Domain.Users.Connections;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Settings;
using Rise.Domain.Users.Settings.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        new User()
        {
            AccountId = "valid-id-" + id,
            FirstName = ValidFirstName(),
            LastName = ValidLastName(),
            Biography = ValidBiography(),
            AvatarUrl = ValidAvatarUrl(),
            BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-28)),
            Gender = GenderType.X,
            UserSettings = ValidUserSettings()
        }.WithId(id);

    public static Supervisor ValidSupervisor(int id) =>
        new Supervisor()
        {
            AccountId = "valid-id-" + id,
            FirstName = ValidFirstName(),
            LastName = ValidLastName(),
            Biography = ValidBiography(),
            AvatarUrl = ValidAvatarUrl(),
            BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-40)),
            Gender = GenderType.X,
            UserSettings = ValidUserSettings()
        }.WithId(id);

    private static T WithId<T>(this T user, int id) where T : BaseUser
    {
        typeof(T)
            .GetProperty(nameof(BaseUser.Id))!
            .SetValue(user, id);

        return user;
    }
    public static User WithConnections(this User user, UserConnection connection)
    {
        return user.WithConnections([connection]);
    }
    public static User WithConnections(this User user, IEnumerable<UserConnection> connections)
    {
        typeof(User)
            .GetField("_connections", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(user, new HashSet<UserConnection>(connections));

        return user;
    }

    public static Chat ValidChat()
    {
        var user1 = ValidUser(-1);
        var user2 = ValidUser(-2);

        user1.AcceptFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        return Chat.CreateChat(user1, user2);
    }
}
