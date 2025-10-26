using System;
using Rise.Domain.Users.Hobbys;
using Rise.Shared.Users;

namespace Rise.Services.Users.Mapper;

internal static class HobbyMapper
{
    public static UserHobbyDto ToDto(UserHobby hobby)
    {
        var (name, emoji) = Translate(hobby.Hobby);

        return new UserHobbyDto
        {
            Id = hobby.Hobby.ToString(),
            Name = name,
            Emoji = emoji,
        };
    }

    private static (string Name, string Emoji) Translate(HobbyType hobby) => hobby switch
    {
        HobbyType.Swimming => ("Zwemmen", "🏊"),
        HobbyType.Football => ("Voetbal", "⚽"),
        HobbyType.Rugby => ("Rugby", "🏉"),
        HobbyType.Basketball => ("Basketbal", "🏀"),
        HobbyType.Gaming => ("Gamen", "🎮"),
        HobbyType.Cooking => ("Koken", "🍳"),
        HobbyType.Baking => ("Bakken", "🧁"),
        HobbyType.Hiking => ("Wandelen in de natuur", "🥾"),
        HobbyType.Cycling => ("Fietsen", "🚴"),
        HobbyType.Drawing => ("Tekenen", "✏️"),
        HobbyType.Painting => ("Schilderen", "🎨"),
        HobbyType.MusicMaking => ("Muziek maken", "🎶"),
        HobbyType.Singing => ("Zingen", "🎤"),
        HobbyType.Dancing => ("Dansen", "💃"),
        HobbyType.Reading => ("Lezen", "📚"),
        HobbyType.Gardening => ("Tuinieren", "🌱"),
        HobbyType.Fishing => ("Vissen", "🎣"),
        HobbyType.Camping => ("Kamperen", "🏕️"),
        HobbyType.Photography => ("Fotografie", "📸"),
        HobbyType.Crafting => ("Knutselen", "✂️"),
        HobbyType.Sewing => ("Naaien", "🧵"),
        HobbyType.Knitting => ("Breien", "🧶"),
        HobbyType.Woodworking => ("Houtbewerking", "🪚"),
        HobbyType.Pottery => ("Keramiek", "🏺"),
        HobbyType.Writing => ("Verhalen schrijven", "✍️"),
        HobbyType.Birdwatching => ("Vogels spotten", "🐦"),
        HobbyType.ModelBuilding => ("Modelbouw", "🧱"),
        HobbyType.Chess => ("Schaken", "♟️"),
        HobbyType.BoardGames => ("Bordspellen", "🎲"),
        HobbyType.Puzzles => ("Puzzels leggen", "🧩"),
        HobbyType.CardGames => ("Kaartspellen", "🃏"),
        HobbyType.Running => ("Hardlopen", "🏃"),
        HobbyType.Yoga => ("Yoga", "🧘"),
        HobbyType.Pilates => ("Pilates", "🤸"),
        HobbyType.Skating => ("Skeeleren", "⛸️"),
        HobbyType.Bouldering => ("Boulderen", "🧗"),
        _ => throw new ArgumentOutOfRangeException(nameof(hobby), hobby, "No descriptor configured for hobby type."),
    };
}
