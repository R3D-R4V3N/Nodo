namespace Rise.Domain.Users.Hobbys;

public class UserHobby : Entity
{
    public required HobbyType Hobby { get; set; }
}

public static class HobbyCatalog
{
    public static HobbyDescriptor GetDescriptor(HobbyType hobby) => hobby switch
    {
        HobbyType.Swimming => new("Zwemmen", "🏊"),
        HobbyType.Football => new("Voetbal", "⚽"),
        HobbyType.Rugby => new("Rugby", "🏉"),
        HobbyType.Basketball => new("Basketbal", "🏀"),
        HobbyType.Gaming => new("Gamen", "🎮"),
        HobbyType.Cooking => new("Koken", "🍳"),
        HobbyType.Baking => new("Bakken", "🧁"),
        HobbyType.Hiking => new("Wandelen in de natuur", "🥾"),
        HobbyType.Cycling => new("Fietsen", "🚴"),
        HobbyType.Drawing => new("Tekenen", "✏️"),
        HobbyType.Painting => new("Schilderen", "🎨"),
        HobbyType.MusicMaking => new("Muziek maken", "🎶"),
        HobbyType.Singing => new("Zingen", "🎤"),
        HobbyType.Dancing => new("Dansen", "💃"),
        HobbyType.Reading => new("Lezen", "📚"),
        HobbyType.Gardening => new("Tuinieren", "🌱"),
        HobbyType.Fishing => new("Vissen", "🎣"),
        HobbyType.Camping => new("Kamperen", "🏕️"),
        HobbyType.Photography => new("Fotografie", "📸"),
        HobbyType.Crafting => new("Knutselen", "✂️"),
        HobbyType.Sewing => new("Naaien", "🧵"),
        HobbyType.Knitting => new("Breien", "🧶"),
        HobbyType.Woodworking => new("Houtbewerking", "🪚"),
        HobbyType.Pottery => new("Keramiek", "🏺"),
        HobbyType.Writing => new("Verhalen schrijven", "✍️"),
        HobbyType.Birdwatching => new("Vogels spotten", "🐦"),
        HobbyType.ModelBuilding => new("Modelbouw", "🧱"),
        HobbyType.Chess => new("Schaken", "♟️"),
        HobbyType.BoardGames => new("Bordspellen", "🎲"),
        HobbyType.Puzzles => new("Puzzels leggen", "🧩"),
        HobbyType.CardGames => new("Kaartspellen", "🃏"),
        HobbyType.Running => new("Hardlopen", "🏃"),
        HobbyType.Yoga => new("Yoga", "🧘"),
        HobbyType.Pilates => new("Pilates", "🤸"),
        HobbyType.Skating => new("Skeeleren", "⛸️"),
        HobbyType.Bouldering => new("Boulderen", "🧗"),
        _ => throw new ArgumentOutOfRangeException(nameof(hobby), hobby, "No descriptor configured for hobby type.")
    };
}

public record HobbyDescriptor(string Name, string Emoji);
