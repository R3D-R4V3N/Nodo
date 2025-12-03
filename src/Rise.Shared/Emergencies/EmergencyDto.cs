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
    }

    public static (string Label, string IconHref) TranslateTypeToDisplay(EmergencyTypeDto type) => type switch
    {
        EmergencyTypeDto.Bullying => ("Pesten", "/images/discrimination.png"),
        EmergencyTypeDto.Exclusion => ("Uitsluiten", "/images/bully.png"),
        EmergencyTypeDto.InappropriateLanguage => ("Ongepast taalgebruik", "/images/cyber-bullying.png"),
        EmergencyTypeDto.Other => ("Ander probleem", "/images/menu.png"),
        _ => ("Ander probleem", "/images/menu.png")
    };
}
