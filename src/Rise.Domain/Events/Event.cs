using Rise.Domain.Users;

namespace Rise.Domain.Events;

public class Event: Entity
{
    public string Name { get; set; }
    
    public DateTime Date { get; set; }
    
    public string Location { get; set; }
    
    public Double Price { get; set; }
    
    public string ImageUrl { get; set; }
    
    public List<User> InterestedUsers { get; set; }
    
}