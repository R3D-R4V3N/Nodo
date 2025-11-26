namespace Rise.Shared.Events;

public static class EventDto
{
    public class Get
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public DateTime Date { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public Double Price { get; set; }

        public string ImageUrl { get; set; }

        public List<InterestedUser> InterestedUsers { get; set; } = [];
    }

    public class InterestedUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
    }
}