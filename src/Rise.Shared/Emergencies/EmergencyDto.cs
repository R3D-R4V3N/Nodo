namespace Rise.Shared.Emergencies;

public static class EmergencyDto
{
    public class Create
    {
        public required string Message { get; set; }
    }
    public class Get 
    {
        public int Id { get; set; }
    }
}
