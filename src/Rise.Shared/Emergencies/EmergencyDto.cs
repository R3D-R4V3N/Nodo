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
        public EmergencyTypeDto Type { get; set; }
        public int ChatId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Reporter { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public string Label => TranslateEnumToText(Type).Label;
        public string IconHref => TranslateEnumToText(Type).IconHref;
    }

    public static (string Label, string IconHref) TranslateEnumToText(EmergencyTypeDto emergency) => emergency switch
    {
        EmergencyTypeDto.Threat => ("Bedreiging", "/images/bully.png"),
        EmergencyTypeDto.SwearWord => ("Ongepast taalgebruik", "/images/cyber-bullying.png"),
        EmergencyTypeDto.Other => ("Ander probleem", "/images/menu.png"),
        _ => throw new ArgumentOutOfRangeException(nameof(emergency), emergency, "No translation configured for emergency type."),
    };
}
