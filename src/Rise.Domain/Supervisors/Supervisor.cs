using Rise.Domain.Chats;

namespace Rise.Domain.Supervisors;

public class Supervisor : ChatUser
{
    /// <summary>
    /// Link to the <see cref="IdentityUser"/> account, so a Technician HAS A Account and not IS A <see cref="IdentityUser"/>./>
    /// </summary>
    public required string AccountId { get; init; }

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string ServiceName { get; set; }
}
