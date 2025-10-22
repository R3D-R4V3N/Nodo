using System;
using System.Collections.Generic;
using System.Linq;

namespace Rise.Shared.Profile;

public static class ProfileCatalog
{
    public const int MaxInterestCount = 3;

    private static readonly IReadOnlyList<ProfileInterestDto> _interests =
    [
        new("zwemmen", "Zwemmen", "🏊"),
        new("voetbal", "Voetbal", "⚽"),
        new("rugby", "Rugby", "🏉"),
        new("basketbal", "Basketbal", "🏀"),
        new("gaming", "Gaming", "🎮"),
        new("koken", "Koken", "🍳"),
        new("bakken", "Bakken", "🧁"),
        new("wandelen", "Wandelen", "🚶"),
        new("fietsen", "Fietsen", "🚴"),
        new("tekenen", "Tekenen", "✏️"),
        new("schilderen", "Schilderen", "🎨"),
        new("muziek", "Muziek", "🎵"),
        new("zingen", "Zingen", "🎤"),
        new("dansen", "Dansen", "🕺"),
        new("lezen", "Lezen", "📚"),
        new("tuinieren", "Tuinieren", "🌱"),
        new("vissen", "Vissen", "🎣"),
        new("kamperen", "Kamperen", "🎕️"),
        new("reizen", "Reizen", "✈️"),
        new("fotografie", "Fotografie", "📸"),
        new("film", "Film", "🎬"),
        new("series", "Series", "📺"),
        new("dieren", "Dieren", "🐶"),
        new("yoga", "Yoga", "🧘‍♂️"),
        new("fitness", "Fitness", "🏋️‍♂️"),
        new("hardlopen", "Hardlopen", "🏃‍♂️"),
        new("kaarten", "Kaarten", "🃏"),
        new("puzzelen", "Puzzelen", "🧩"),
        new("bordspellen", "Bordspellen", "🎲"),
        new("knutselen", "Knutselen", "✂️")
    ];

    public static IReadOnlyList<ProfileInterestDto> Interests => _interests;

    public static bool IsValidInterest(string id)
        => _interests.Any(i => string.Equals(i.Id, id, StringComparison.OrdinalIgnoreCase));
}

public record ProfileInterestDto(string Id, string Name, string Emoji);
