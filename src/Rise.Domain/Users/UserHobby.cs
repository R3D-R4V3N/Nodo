using System.Collections.Generic;
using Ardalis.GuardClauses;
using Rise.Domain.Common;

namespace Rise.Domain.Users;

public class UserHobby : ValueObject
{
    public required HobbyType Hobby { get; set; }

    public static UserHobby Create(HobbyType hobby)
        => new() { Hobby = Guard.Against.Default(hobby) };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Hobby;
    }
}

public enum HobbyType
{
    Swimming,
    Football,
    Rugby,
    Basketball,
    Gaming,
    Cooking,
    Baking,
    Hiking,
    Cycling,
    Drawing,
    Painting,
    MusicMaking,
    Singing,
    Dancing,
    Reading,
    Gardening,
    Fishing,
    Camping,
    Photography,
    Yoga,
    Running,
    Crafting,
    Sewing,
    Knitting,
    Woodworking,
    Pottery,
    Writing,
    Birdwatching,
    ModelBuilding,
    Chess,
    BoardGames,
    Puzzles,
    CardGames,
    Pilates,
    Skating,
    Bouldering,
}

public static class HobbyCatalog
{
    private static readonly IReadOnlyDictionary<HobbyType, HobbyDescriptor> _descriptors =
        new Dictionary<HobbyType, HobbyDescriptor>
        {
            [HobbyType.Swimming] = new("Zwemmen", "🏊"),
            [HobbyType.Football] = new("Voetbal", "⚽"),
            [HobbyType.Rugby] = new("Rugby", "🏉"),
            [HobbyType.Basketball] = new("Basketbal", "🏀"),
            [HobbyType.Gaming] = new("Gamen", "🎮"),
            [HobbyType.Cooking] = new("Koken", "🍳"),
            [HobbyType.Baking] = new("Bakken", "🧁"),
            [HobbyType.Hiking] = new("Wandelen in de natuur", "🥾"),
            [HobbyType.Cycling] = new("Fietsen", "🚴"),
            [HobbyType.Drawing] = new("Tekenen", "✏️"),
            [HobbyType.Painting] = new("Schilderen", "🎨"),
            [HobbyType.MusicMaking] = new("Muziek maken", "🎶"),
            [HobbyType.Singing] = new("Zingen", "🎤"),
            [HobbyType.Dancing] = new("Dansen", "💃"),
            [HobbyType.Reading] = new("Lezen", "📚"),
            [HobbyType.Gardening] = new("Tuinieren", "🌱"),
            [HobbyType.Fishing] = new("Vissen", "🎣"),
            [HobbyType.Camping] = new("Kamperen", "🏕️"),
            [HobbyType.Photography] = new("Fotografie", "📸"),
            [HobbyType.Crafting] = new("Knutselen", "✂️"),
            [HobbyType.Sewing] = new("Naaien", "🧵"),
            [HobbyType.Knitting] = new("Breien", "🧶"),
            [HobbyType.Woodworking] = new("Houtbewerking", "🪚"),
            [HobbyType.Pottery] = new("Keramiek", "🏺"),
            [HobbyType.Writing] = new("Verhalen schrijven", "✍️"),
            [HobbyType.Birdwatching] = new("Vogels spotten", "🐦"),
            [HobbyType.ModelBuilding] = new("Modelbouw", "🧱"),
            [HobbyType.Chess] = new("Schaken", "♟️"),
            [HobbyType.BoardGames] = new("Bordspellen", "🎲"),
            [HobbyType.Puzzles] = new("Puzzels leggen", "🧩"),
            [HobbyType.CardGames] = new("Kaartspellen", "🃏"),
            [HobbyType.Running] = new("Hardlopen", "🏃"),
            [HobbyType.Yoga] = new("Yoga", "🧘"),
            [HobbyType.Pilates] = new("Pilates", "🤸"),
            [HobbyType.Skating] = new("Skeeleren", "⛸️"),
            [HobbyType.Bouldering] = new("Boulderen", "🧗"),
        };

    public static HobbyDescriptor GetDescriptor(HobbyType hobby)
    {
        if (_descriptors.TryGetValue(hobby, out var descriptor))
        {
            return descriptor;
        }

        throw new KeyNotFoundException($"No descriptor configured for hobby type '{hobby}'.");
    }
}

public record HobbyDescriptor(string Name, string Emoji);
