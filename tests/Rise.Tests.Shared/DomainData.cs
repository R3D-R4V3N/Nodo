using Rise.Domain.Chats;
using Rise.Domain.Common;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Emergencies;
using Rise.Domain.Events;
using Rise.Domain.Organizations;
using Rise.Domain.Users;
using Rise.Domain.Users.Connections;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;
using Rise.Domain.Users.Settings;
using System.Reflection;
using System.Xml.Schema;

namespace Rise.Tests.Shared;

public static class DomainData
{
    private static readonly Random _random = new();
    private static int _index = 1;
    private static int GetValidIndex()
    {
        return Interlocked.Increment(ref _index);
    }

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
    public static BlobUrl ValidAvatarUrl() => BlobUrl.Create($"Dit is een img.");
    public static BirthDay ValidBirthDay() => BirthDay.Create(DateOnly.FromDateTime(DateTime.Today.AddYears(-28)));
    public static Organization ValidOrganization()
        => new Organization("Nodo Centrum", "Ondersteuning vanuit het centrale team.");
    public static User ValidUser() 
    {
        int id = GetValidIndex();
        return new User()
        {
            AccountId = "valid-id-" + id,
            FirstName = ValidFirstName(),
            LastName = ValidLastName(),
            Biography = ValidBiography(),
            AvatarUrl = ValidAvatarUrl(),
            BirthDay = ValidBirthDay(),
            Gender = GenderType.X,
            UserSettings = ValidUserSettings(),
            Organization = ValidOrganization(),
            Supervisor = ValidSupervisor(),
        }.WithId(id);
    }

    public static Supervisor ValidSupervisor()
    {
        int id = GetValidIndex();
        return new Supervisor()
        {
            AccountId = "valid-id-" + id,
            FirstName = ValidFirstName(),
            LastName = ValidLastName(),
            Biography = ValidBiography(),
            AvatarUrl = ValidAvatarUrl(),
            BirthDay = ValidBirthDay(),
            Gender = GenderType.X,
            UserSettings = ValidUserSettings(),
            Organization = ValidOrganization()
        }.WithId(id);
    }

    public static string ValidNaughtyWord()
    {
        var field = typeof(WordFilter)
              .GetField("_blacklistedWords", BindingFlags.NonPublic | BindingFlags.Static)!;

        var set = field.GetValue(null) as HashSet<string> ?? [];

        if (set == null || set.Count == 0)
            return string.Empty;

        return set.ElementAt(_random.Next(set.Count));
    }

    public static Event ValidEvent()
    {
        int id = GetValidIndex();
        return new Event
        {
            Name = "Event Name",
            Date = DateTime.Now.AddDays(8).Date.AddHours(20),
            Location = "Location",
            Price = 100.00,
            ImageUrl = "Url",
            InterestedUsers = []
        }.WithId(id);
    }

    public static Emergency ValidEmergency()
    {
        var id = GetValidIndex();
        var chat = ValidChat();
        var chatUser = chat.Users.First();
        var message = chat.AddTextMessage("test", chatUser).Value;

        return new Emergency
        {   
            HappenedInChat = chat,
            MadeByUser = chatUser,
            Range = EmergencyRange.Create(DateTime.UtcNow),
            Type = EmergencyType.Other
        }.WithId(id);
    }

    public static Event WithUsers(this Event e, params User[] users)
    {
        e.InterestedUsers.AddRange(users);
        return e;
    }

    public static T WithId<T>(this T entity, int id) where T : Entity
    {
        typeof(T)
            .GetProperty(nameof(Entity.Id))!
            .SetValue(entity, id);

        return entity;
    }
    public static User WithConnections(this User user, UserConnection connection)
    {
        return user.WithConnections([connection]);
    }
    public static User WithConnections(this User user, IEnumerable<UserConnection> connections)
    {
        typeof(User)
            .GetField("_connections", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(user, new List<UserConnection>(connections));

        return user;
    }
    public static User WithSentiments(this User user, IEnumerable<UserSentiment> sentiments)
    {
        typeof(User)
            .GetField("_sentiments", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(user, new List<UserSentiment>(sentiments));

        return user;
    }
    public static User WithHobbies(this User user, IEnumerable<UserHobby> hobbies)
    {
        typeof(User)
            .GetField("_hobbies", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(user, new List<UserHobby>(hobbies));

        return user;
    }
    public static Chat ValidChat()
    {
        var user1 = ValidUser();
        var user2 = ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        return Chat.CreatePrivateChat(user1, user2);
    }

    public static string GetRandomBlacklistedWord() 
    {
        var field = typeof(WordFilter).GetField("_blacklistedWords",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var set = field.GetValue(null) as HashSet<string>;

        if (set == null || set.Count == 0)
            return string.Empty;

        int index = new Random().Next(set.Count);
        return set.ElementAt(index);
    }
}
