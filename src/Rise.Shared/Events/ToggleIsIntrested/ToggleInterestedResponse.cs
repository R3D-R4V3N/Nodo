namespace Rise.Shared.Events;

public static partial class EventResponse
{
    public class ToggleInterest
    {
        public bool IsInterested { get; set; }
        public int InterestedCount { get; set; }
    }
}
