using Rise.Domain.Supervisors;
using Rise.Domain.Users;

namespace Rise.Domain.Chats;

public class Emergency : Entity
{
    public DateTime SnapshotStartTime { get => SnapshotEndTime.AddHours(-24); }
    public required DateTime SnapshotEndTime { get; set; }
    public bool HasBeenResolved { get; set; }
    public required IChat HappenedIn { get; set; }
    public required ApplicationUser NotifiedBy { get; set; }
    public required List<Supervisor> HandledBy { get; set; }
}
