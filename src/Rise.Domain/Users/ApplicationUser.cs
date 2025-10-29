using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Rise.Domain.Chats;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;

namespace Rise.Domain.Users;

public class ApplicationUser : Entity
{
    /// <summary>
    /// Link to the <see cref="IdentityUser"/> account so chatprofielen gekoppeld blijven aan hun login.
    /// </summary>
    public string AccountId { get; private set; }

    private string _firstName = string.Empty;
    public required string FirstName
    {
        get => _firstName;
        set => _firstName = Guard.Against.NullOrWhiteSpace(value);
    }
    private string _lastName = string.Empty;
    public required string LastName
    {
        get => _lastName;
        set => _lastName = Guard.Against.NullOrWhiteSpace(value);
    }
    private string _biography = string.Empty;
    public required string Biography
    {
        get => _biography;
        set => _biography = Guard.Against.NullOrWhiteSpace(value);
    }
    private string _avatarUrl = string.Empty;
    public required string AvatarUrl
    {
        get => _avatarUrl;
        set => _avatarUrl = Guard.Against.NullOrWhiteSpace(value);
    }
    private string _gender = "x";
    public required string Gender
    {
        get => _gender;
        set => _gender = Guard.Against.NullOrWhiteSpace(value);
    }
    public required DateOnly BirthDay { get; set; }
    public required UserType UserType { get; set; }
    

    // sentiments
    private readonly List<UserSentiment> _sentiments = [];
    public IReadOnlyCollection<UserSentiment> Sentiments => _sentiments;
    public IEnumerable<UserSentiment> Likes => _sentiments
        .Where(x => x.Type.Equals(SentimentType.Like));
    public IEnumerable<UserSentiment> Dislikes => _sentiments
        .Where(x => x.Type.Equals(SentimentType.Dislike));

    public Result UpdateSentiments(IEnumerable<UserSentiment> sentiments)
    {
        if (sentiments is null)
            return Result.Conflict("Gevoelens is null");

        const int MAX_SENTIMENT_TYPE = 5;
        // if performance becomes an issue and you know SentimentType
        // will be nicely indexed, use stackalloc
        Dictionary<SentimentType, int> freq = Enum.GetValues<SentimentType>()
            .ToDictionary(x => x, _ => 0);

        List<UserSentiment> tempLst = [];

        foreach (var sentiment in sentiments)
        {
            if (sentiment is null)
                return Result.Conflict("Gevoelens is null");

            if (tempLst.Contains(sentiment))
                continue;

            if (++freq[sentiment.Type] > MAX_SENTIMENT_TYPE)
                return Result.Conflict($"Mag maximaal {MAX_SENTIMENT_TYPE} van een gevoelens type hebben, {sentiment.Type} overschreed dit");

            var hasConflictingSentiment = tempLst
                .Any(x => x.Type != sentiment.Type 
                    && x.Category == sentiment.Category);

            if (hasConflictingSentiment)
                return Result.Conflict("Bevat duplicaat category in een andere gevoel");

            tempLst.Add(sentiment);
        }

        _sentiments.Clear();
        _sentiments.AddRange(tempLst);

        return Result.Success();
    }

    // hobbies
    private readonly HashSet<UserHobby> _hobbies = [];
    public IReadOnlyCollection<UserHobby> Hobbies => _hobbies;

    public void UpdateHobbies(IEnumerable<UserHobby> hobbies)
    {
        Guard.Against.Null(hobbies);

        _hobbies.Clear();
        foreach (var hobby in hobbies)
        {
            _hobbies.Add(Guard.Against.Null(hobby));
        }
    }

    //// connections
    private readonly HashSet<UserConnection> _connections = [];
    public IReadOnlyCollection<UserConnection> Connections => _connections;
    public IEnumerable<UserConnection> Friends => _connections
        .Where(x => x.ConnectionType.Equals(UserConnectionType.Friend));
    public IEnumerable<UserConnection> FriendRequests => _connections
        .Where(x => 
            x.ConnectionType.Equals(UserConnectionType.RequestIncoming) 
            || x.ConnectionType.Equals(UserConnectionType.RequestOutgoing));
    public IEnumerable<UserConnection> BlockedUsers => _connections
        .Where(x => x.ConnectionType.Equals(UserConnectionType.Blocked));

    // chats
    private readonly List<Chat> _chats = [];
    public IReadOnlyList<Chat> Chats => _chats.AsReadOnly();

    // settings
    private ApplicationUserSetting _userSettings;
    public required ApplicationUserSetting UserSettings
    {
        get => _userSettings;
        set
        {
            if (_userSettings == value) return;

            _userSettings = Guard.Against.Null(value);
            if (_userSettings.User != this)
            {
                _userSettings.User = this;
            }
        }
    }

    public ApplicationUser()
    {
    }

    public ApplicationUser(string accountId)
    {
        AccountId = Guard.Against.NullOrEmpty(accountId);
    }

    public bool HasFriend(ApplicationUser friend) 
        => _connections.Contains(new UserConnection() { Connection = friend, ConnectionType = UserConnectionType.Friend});

    public Result<string> AddFriend(ApplicationUser friend)
    {
        bool isAdded = Friends
            .Any(x => x.Connection.Equals(friend));

        if (isAdded)
        { 
            return Result.Conflict($"Gebruiker is al bevriend met {friend}");
        }

        UserConnection? friendRequest = FriendRequests
            .FirstOrDefault(x => x.Connection.Equals(friend));

        if (friendRequest is null)
        {
            _connections.Add(
                new UserConnection()
                {
                    Connection = friend,
                    ConnectionType = UserConnectionType.RequestOutgoing
                }
            );

            friend._connections.Add(
                new UserConnection() 
                { 
                    Connection = this,
                    ConnectionType = UserConnectionType.RequestIncoming 
                }
            );

            return Result.Success($"Gebruiker verstuurd een vriendschapsverzoek naar {friend}");
        }

        if (friendRequest.ConnectionType.Equals(UserConnectionType.RequestOutgoing))
        {
            return Result.Conflict($"Gebruiker heeft al een vriendschapsverzoek naar {friend} verstuurd");
        }

        _connections.Add(new 
            UserConnection() 
            { 
                Connection = friend, 
                ConnectionType = UserConnectionType.Friend 
            }
        );

        _connections.Remove(
            new UserConnection()
            { 
                Connection = friend, 
                ConnectionType = UserConnectionType.RequestIncoming 
            }
        );

        friend._connections.Add(new
            UserConnection()
            {
                Connection = this,
                ConnectionType = UserConnectionType.Friend
            }
        );

        friend._connections.Remove(
            new UserConnection()
            {
                Connection = this,
                ConnectionType = UserConnectionType.RequestOutgoing
            }
        );

        return Result.Success($"Gebruiker voegt {friend} toe");
    }

    public Result RemoveFriend(ApplicationUser friend)
    {
        _connections.Remove(
            new UserConnection()
            {
                Connection = friend,
                ConnectionType = UserConnectionType.Friend,
            }
        );

        friend._connections.Remove(
            new UserConnection()
            {
                Connection = this,
                ConnectionType = UserConnectionType.Friend
            }
        );

        return Result.Success();
    }

    public Result AddChat(Chat chat)
    {
        if (_chats.Contains(chat))
        {
            return Result.Conflict($"Gebruiker is al lid van chat {chat}");
        }

        _chats.Add(chat);
        chat.AddUser(this);

        return Result.Success();
    }
    public Result RemoveChat(Chat chat)
    {
        if (!_chats.Contains(chat))
        {
            return Result.Conflict($"Gebruiker is geen lid van chat {chat}");
        }

        _chats.Remove(chat);
        chat.RemoveUser(this);

        return Result.Success();
    }
}

