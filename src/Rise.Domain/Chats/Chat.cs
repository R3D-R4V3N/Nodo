using System;
using Ardalis.GuardClauses;
using Rise.Domain.Users;

namespace Rise.Domain.Chats;

public class Chat : Entity
{
    private List<Message> _messages = [];
    private List<ChatParticipant> _participants = [];

    public List<Message> Messages
    {
        get => _messages;
        set => _messages = value ?? throw new ArgumentNullException(nameof(value));
    }

    public List<ChatParticipant> Participants
    {
        get => _participants;
        set => _participants = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool IsEmergencyActive { get; private set; }

    public int? EmergencyInitiatorId { get; private set; }

    public DateTime? EmergencyActivatedAtUtc { get; private set; }

    public void ActivateEmergency(ApplicationUser initiator, DateTime utcNow)
    {
        Guard.Against.Null(initiator);

        if (IsEmergencyActive)
        {
            return;
        }

        IsEmergencyActive = true;
        EmergencyInitiatorId = initiator.Id;
        EmergencyActivatedAtUtc = utcNow;
    }

    public void DeactivateEmergency(ApplicationUser initiator)
    {
        Guard.Against.Null(initiator);

        if (!IsEmergencyActive)
        {
            return;
        }

        Guard.Against.InvalidInput(initiator.Id, nameof(initiator), id => EmergencyInitiatorId == id, "Alleen de melder kan de noodmelding intrekken.");

        IsEmergencyActive = false;
        EmergencyInitiatorId = null;
        EmergencyActivatedAtUtc = null;
    }
}