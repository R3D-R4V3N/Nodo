namespace Rise.Shared.Events.GetEvents;

public static partial class GetEventsResponse
{
    public class GetEvents
    {
        public IEnumerable<EventDto.Get> Events { get; set; } = [];
        public int TotalCount { get; set; }
    }
}