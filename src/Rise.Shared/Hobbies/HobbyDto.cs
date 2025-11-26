namespace Rise.Shared.Hobbies;

public static class HobbyDto
{
    public record Get
    {
        public int Id { get; set; }
        public HobbyTypeDto Hobby { get; init; }
        public string Name => TranslateEnumToText(Hobby).Name;
        public string Emoji => TranslateEnumToText(Hobby).Emoji;
    }

    public record EditProfile
    {
        public HobbyTypeDto Hobby { get; init; }
    }

    public static (string Name, string Emoji) TranslateEnumToText(HobbyTypeDto hobby) => hobby switch
    {
        HobbyTypeDto.Swimming => ("Zwemmen", "🏊"),
        HobbyTypeDto.Football => ("Voetbal", "⚽"),
        HobbyTypeDto.Rugby => ("Rugby", "🏉"),
        HobbyTypeDto.Basketball => ("Basketbal", "🏀"),
        HobbyTypeDto.Gaming => ("Gamen", "🎮"),
        HobbyTypeDto.Cooking => ("Koken", "🍳"),
        HobbyTypeDto.Baking => ("Bakken", "🧁"),
        HobbyTypeDto.Hiking => ("Wandelen in de natuur", "🥾"),
        HobbyTypeDto.Cycling => ("Fietsen", "🚴"),
        HobbyTypeDto.Drawing => ("Tekenen", "✏️"),
        HobbyTypeDto.Painting => ("Schilderen", "🎨"),
        HobbyTypeDto.MusicMaking => ("Muziek maken", "🎶"),
        HobbyTypeDto.Singing => ("Zingen", "🎤"),
        HobbyTypeDto.Dancing => ("Dansen", "💃"),
        HobbyTypeDto.Reading => ("Lezen", "📚"),
        HobbyTypeDto.Gardening => ("Tuinieren", "🌱"),
        HobbyTypeDto.Fishing => ("Vissen", "🎣"),
        HobbyTypeDto.Camping => ("Kamperen", "🏕️"),
        HobbyTypeDto.Photography => ("Fotografie", "📸"),
        HobbyTypeDto.Crafting => ("Knutselen", "✂️"),
        HobbyTypeDto.Sewing => ("Naaien", "🧵"),
        HobbyTypeDto.Knitting => ("Breien", "🧶"),
        HobbyTypeDto.Woodworking => ("Houtbewerking", "🪚"),
        HobbyTypeDto.Pottery => ("Keramiek", "🏺"),
        HobbyTypeDto.Writing => ("Verhalen schrijven", "✍️"),
        HobbyTypeDto.Birdwatching => ("Vogels spotten", "🐦"),
        HobbyTypeDto.ModelBuilding => ("Modelbouw", "🧱"),
        HobbyTypeDto.Chess => ("Schaken", "♟️"),
        HobbyTypeDto.BoardGames => ("Bordspellen", "🎲"),
        HobbyTypeDto.Puzzles => ("Puzzels leggen", "🧩"),
        HobbyTypeDto.CardGames => ("Kaartspellen", "🃏"),
        HobbyTypeDto.Running => ("Hardlopen", "🏃"),
        HobbyTypeDto.Yoga => ("Yoga", "🧘"),
        HobbyTypeDto.Pilates => ("Pilates", "🤸"),
        HobbyTypeDto.Skating => ("Skeeleren", "⛸️"),
        HobbyTypeDto.Bouldering => ("Boulderen", "🧗"),
        _ => throw new ArgumentOutOfRangeException(nameof(hobby), hobby, "No translation configured for hobby type."),
    };
}
