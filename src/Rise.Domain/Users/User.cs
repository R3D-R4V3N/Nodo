using Ardalis.Result;
using Rise.Domain.Chats;
using Rise.Domain.Events;
using Rise.Domain.Helper;
using Rise.Domain.Organizations;
using Rise.Domain.Users.Connections;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;
using Rise.Domain.Users.Settings;
using System.Collections.Generic;

namespace Rise.Domain.Users;

public class User : BaseUser
{
    public const int MAX_SENTIMENTS_PER_TYPE = 5;
    public const int MAX_HOBBIES = 3;

    // sentiments
    private readonly List<UserSentiment> _sentiments = [];
    public IReadOnlyCollection<UserSentiment> Sentiments => _sentiments;
    public IEnumerable<UserSentiment> Likes => _sentiments
        .Where(x => x.Type.Equals(SentimentType.Like));
    public IEnumerable<UserSentiment> Dislikes => _sentiments
        .Where(x => x.Type.Equals(SentimentType.Dislike));

    // hobbies
    private readonly List<UserHobby> _hobbies = [];
    public IReadOnlyCollection<UserHobby> Hobbies => _hobbies;

    //// connections
    private readonly List<UserConnection> _connections = [];
    public IReadOnlyCollection<UserConnection> Connections => _connections;
    public IEnumerable<UserConnection> Friends => _connections
        .Where(x => x.ConnectionType is UserConnectionType.Friend);
    public IEnumerable<UserConnection> FriendRequests => _connections
        .Where(x => x.ConnectionType is UserConnectionType.RequestIncoming or UserConnectionType.RequestOutgoing);
    public IEnumerable<UserConnection> BlockedUsers => _connections
        .Where(x => x.ConnectionType is UserConnectionType.Blocked);

    // events
    private readonly List<Event> _interestedInEvents = [];
    public IReadOnlyCollection<Event> InterestedInEvents => _interestedInEvents;

    public bool IsFriend(User req) 
        => (GetConnection(req)?.ConnectionType ?? UserConnectionType.None) == UserConnectionType.Friend;
    public bool IsBlocked(User req)
        => (GetConnection(req)?.ConnectionType ?? UserConnectionType.None) == UserConnectionType.Blocked;
    public bool HasFriendRequest(User req)
    { 
        var type = GetConnection(req)?.ConnectionType ?? UserConnectionType.None;
        return type.Equals(UserConnectionType.RequestIncoming) || type.Equals(UserConnectionType.RequestOutgoing);
    }

    // orga
    private Organization _organization;
    public required Organization Organization
    {
        get => _organization;
        set
        {
            if (_organization == value) return;

            _organization = Guard.Against.Null(value);
            if (!_organization.Members.Contains(this))
            {
                _organization.AddMember(this);
            }
        }
    }

    // supervisor
    private Supervisor _supervisor;
    public required Supervisor Supervisor
    {
        get => _supervisor;
        set
        {
            if (_supervisor == value) return;

            _supervisor = Guard.Against.Null(value);
            if (!_supervisor.Users.Contains(this))
            {
                _supervisor.AddUser(this);
            }
        }
    }

    private UserConnection? GetConnection(User target)
    {
        foreach (var conType in Enum.GetValues<UserConnectionType>())
        {
            var con = _connections.FirstOrDefault(c =>
                c.From.Id == this.Id &&
                c.To.Id == target.Id &&
                c.ConnectionType == conType
            );

            if (con is not null)
                return con;
        }

        return null;
    }

    public Result<User> SendFriendRequest(User target)
    {
        UserConnection? targetConn = target.GetConnection(this);

        if (targetConn is not null && targetConn.ConnectionType.Equals(UserConnectionType.Blocked))
        {
            return Result.Conflict($"{target} heeft je geblokkeerd");
        }

        UserConnection? conn = GetConnection(target);

        if (conn is not null)
        {
            string conflictMessage = conn.ConnectionType switch
            {
                UserConnectionType.Friend
                    => $"{this} is al bevriend met {target}",
                UserConnectionType.RequestIncoming 
                    => $"{target} heeft al een verzoek verstuurd, accepteer hem!",
                UserConnectionType.RequestOutgoing
                    => $"{this} heeft al een vriendschapsverzoek naar {target} verstuurd",
                UserConnectionType.Blocked
                    => $"Gebruiker {target} is geblokkeerd",
                _ => throw new NotImplementedException(conn.ConnectionType.ToString()),
            };

            return Result.Conflict(conflictMessage);
        }

        _connections.Add(
            this.CreateConnectionWith(target, UserConnectionType.RequestOutgoing)
        );

        target._connections.Add(
            target.CreateConnectionWith(this, UserConnectionType.RequestIncoming)
        );

        return Result.Success(this, $"{this} verstuurd een vriendschapsverzoek naar {target}");
    }

    public Result<User> AcceptFriendRequest(User target)
    {
        UserConnection? conn = GetConnection(target);

        if (conn is null)
        {
            return Result.NotFound($"Er is geen veroek van {target} om te accepteren");
        }

        if (conn.ConnectionType.Equals(UserConnectionType.Friend))
        {
            return Result.Conflict($"{this} is al bevriend met {target}");
        }

        if (!conn.ConnectionType.Equals(UserConnectionType.RequestIncoming))
        { 
            return Result.Conflict($"Er is geen verzoek van {target} om te accepteren");
        }

        UserConnection targetConnection = target.GetConnection(this)!;

        conn.ConnectionType = UserConnectionType.Friend;
        targetConnection.ConnectionType = UserConnectionType.Friend;

        return Result.Success(this, $"{this} is nu bevriend met {target}");
    }

    public Result<User> RemoveFriend(User friend)
    {
        UserConnection? conn = GetConnection(friend);

        if (conn is null || conn.ConnectionType != UserConnectionType.Friend)
        {
            return Result.NotFound($"Je bent niet bevriend met {friend}");
        }

        Span<User> span =
        [
            this, friend
        ];

        for (int i = 0; i < span.Length; i++)
        {
            var current = span[i];
            var opposite = span[(i + 1) % span.Length];
            current._connections.Remove(
                current.CreateConnectionWith(opposite, UserConnectionType.Friend)
            );
        }

        return Result.Success(this, $"vriendschap beeindigd met {friend}");
    }
    
    public Result<User> CancelFriendRequest(User target)
    {
        UserConnection? conn = GetConnection(target);

        if (conn is null)
        {
            return Result.NotFound($"Er is geen verzoek naar {target} om te annuleren.");
        }

        if (conn.ConnectionType.Equals(UserConnectionType.Friend))
        {
            return Result.Conflict($"{this} is al bevriend met {target} en kan geen verzoek annuleren.");
        }

        if (!conn.ConnectionType.Equals(UserConnectionType.RequestOutgoing))
        {
            return Result.Conflict($"Er is geen uitgaand verzoek van {this} naar {target} om te annuleren.");
        }

        _connections.Remove(
            this.CreateConnectionWith(target, UserConnectionType.RequestOutgoing)
        );

        target._connections.Remove(
            target.CreateConnectionWith(this, UserConnectionType.RequestIncoming)
        );

        return Result.Success(this, $"{this} heeft het vriendschapsverzoek naar {target} geannuleerd");
    }

    public Result<User> RejectFriendRequest(User target)
    {
        UserConnection? conn = GetConnection(target);

        if (conn is null)
        {
            return Result.NotFound($"Er is geen verzoek van {target} om te weigeren");
        }

        if (conn.ConnectionType.Equals(UserConnectionType.Friend))
        {
            return Result.Conflict($"{this} is al bevriend met {target} en kan geen verzoek weigeren.");
        }

        if (!conn.ConnectionType.Equals(UserConnectionType.RequestIncoming))
        {
            return Result.Conflict($"Er is geen uitgaand verzoek van {target} naar {this} om te weigeren.");
        }

        _connections.Remove(
            this.CreateConnectionWith(target, UserConnectionType.RequestIncoming)
        );

        target._connections.Remove(
            target.CreateConnectionWith(this, UserConnectionType.RequestOutgoing)
        );

        return Result.Success(this, $"{this} heeft het vriendschapsverzoek van {target} geweigerd");
    }

    public Result UpdateSentiments(IEnumerable<UserSentiment> sentiments)
    {
        sentiments ??= [];

        // if performance becomes an issue and you know SentimentType
        // will be nicely indexed, use stackalloc
        Dictionary<SentimentType, int> freq = Enum.GetValues<SentimentType>()
            .ToDictionary(x => x, _ => 0);

        List<UserSentiment> tempLst = [];

        foreach (var sentiment in sentiments)
        {
            if (sentiment is null)
                return Result.Conflict("Gevoelens bevat een null value");

            if (tempLst.Contains(sentiment))
                continue;

            if (++freq[sentiment.Type] > MAX_SENTIMENTS_PER_TYPE)
                return Result.Conflict($"Mag maximaal {MAX_SENTIMENTS_PER_TYPE} van een gevoelens type hebben, {sentiment.Type} overschreed dit");

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

    public Result UpdateHobbies(IEnumerable<UserHobby> hobbies)
    {
        hobbies ??= [];
        
        var hobbiesLst = hobbies as List<UserHobby> ?? hobbies.ToList();

        if (hobbiesLst.Count > MAX_HOBBIES)
            return Result.Conflict($"Mag maximaal {MAX_HOBBIES} hobbies hebben.");

        List<UserHobby> tempLst = [];

        foreach (var hobby in hobbiesLst)
        {
            if (hobby is null)
                return Result.Conflict("Hobbies bevat een null waarde");

            tempLst.Add(hobby);
        }

        _hobbies.Clear();
        _hobbies.AddRange(tempLst);

        return Result.Success();
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName}";
    }
}

