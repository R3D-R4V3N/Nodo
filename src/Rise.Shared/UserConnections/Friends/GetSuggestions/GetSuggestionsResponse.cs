namespace Rise.Shared.UserConnections;


public static partial class UserConnectionResponse
{   
    public class GetSuggestions
    {
        public IEnumerable<UserConnectionDto.Get> Users { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
