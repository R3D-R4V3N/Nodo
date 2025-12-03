using Ardalis.Result;
using Rise.Domain.Chats;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Messages;
using Rise.Domain.Users;

namespace Rise.Domain.Emergencies;

public class Emergency : Entity
{
    //EF
    public Emergency() { }

    public required EmergencyType Type { get; set; } = EmergencyType.Other;
    private Chat _happenedInChat;
    public required Chat HappenedInChat 
    {
        get => _happenedInChat;
        set
        {
            if (_happenedInChat == value) return;
            if (_happenedInChat is not null)
                throw new InvalidOperationException("Noodmelding zijn chat kan niet worden veranderd.");

            value = Guard.Against.Null(value);

            _happenedInChat = value;
            AllowedToResolve = _happenedInChat?
                .Users.OfType<User>()
                .Select(u => u.Supervisor)
                .Distinct()
                .ToList() ?? [];

            if (!value.Emergencies.Contains(this))
            {
                value.AddEmergency(this);
            }
        }
    }
    public required BaseUser MadeByUser { get; set; }
    public required EmergencyRange Range { get; set; }
    public List<Supervisor> AllowedToResolve { get; private set; } = [];
    public List<Supervisor> HasResolved { get; private set; } = [];
    public bool IsResolved => HasResolved.Count >= AllowedToResolve.Count;

    public Result<string> Resolve(Supervisor handler)
    {
        if (IsResolved)
        { 
            return Result.Conflict("Melding werd al opgelost.");
        }

        if (handler is null)
        { 
            return Result.Conflict("Begeleider is leeg.");
        }

        if (!AllowedToResolve.Contains(handler))
        {
            return Result.Conflict($"{handler} heeft geen toestemming om noodmelding op te lossen.");
        }

        if (HasResolved.Contains(handler))
        {
            return Result.Conflict($"{handler} heeft al deze noodmelding opgelost.");
        }

        HasResolved.Add(handler);

        return Result.Success($"Melding werd opgelost door {handler}.");
    }
}
