namespace Rise.Shared.Events;

public static partial class EventResponse
{
    public class GetEvents
    {
        public IEnumerable<EventDto.Get> Events { get; set; } = [];
        public int TotalCount { get; set; }
    }
}