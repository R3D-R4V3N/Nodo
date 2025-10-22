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
    Music,
    Singing,
    Dancing,
    Reading,
    Gardening,
    Fishing,
    Camping,
    Travel,
    Photography,
    Movies,
    Series,
    Animals,
    Yoga,
    Fitness,
    Running,
    Cards,
    Puzzles,
    BoardGames,
    Crafts,
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
            [HobbyType.Gaming] = new("Gaming", "🎮"),
            [HobbyType.Cooking] = new("Koken", "🍳"),
            [HobbyType.Baking] = new("Bakken", "🧁"),
            [HobbyType.Hiking] = new("Wandelen", "🚶"),
            [HobbyType.Cycling] = new("Fietsen", "🚴"),
            [HobbyType.Drawing] = new("Tekenen", "✏️"),
            [HobbyType.Painting] = new("Schilderen", "🎨"),
            [HobbyType.Music] = new("Muziek", "🎵"),
            [HobbyType.Singing] = new("Zingen", "🎤"),
            [HobbyType.Dancing] = new("Dansen", "🕺"),
            [HobbyType.Reading] = new("Lezen", "📚"),
            [HobbyType.Gardening] = new("Tuinieren", "🌱"),
            [HobbyType.Fishing] = new("Vissen", "🎣"),
            [HobbyType.Camping] = new("Kamperen", "🎪"),
            [HobbyType.Travel] = new("Reizen", "✈️"),
            [HobbyType.Photography] = new("Fotografie", "📸"),
            [HobbyType.Movies] = new("Films", "🎬"),
            [HobbyType.Series] = new("Series", "📺"),
            [HobbyType.Animals] = new("Dieren", "🐶"),
            [HobbyType.Yoga] = new("Yoga", "🧘"),
            [HobbyType.Fitness] = new("Fitness", "🏋️"),
            [HobbyType.Running] = new("Hardlopen", "🏃"),
            [HobbyType.Cards] = new("Kaarten", "🃏"),
            [HobbyType.Puzzles] = new("Puzzelen", "🧩"),
            [HobbyType.BoardGames] = new("Bordspellen", "🎲"),
            [HobbyType.Crafts] = new("Knutselen", "✂️"),
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
