using Rise.Domain.Users;

namespace Rise.Domain.Events;

public class Event : Entity
{
    public required string Name { get; set; }
    public required DateTime DateTime { get; set; }
    public required string Location { get; set; }
    public required int Likes { get; set; }
    public List<ApplicationUser> LikedBy { get; set; } = [];
}
