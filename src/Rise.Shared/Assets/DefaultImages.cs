namespace Rise.Shared.Assets;

public static class DefaultImages
{
    private static readonly string[] Profiles =
    {
        "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
        "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
        "https://images.unsplash.com/photo-1544723795-3fb6469f5b39?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
        "https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
        "https://images.unsplash.com/photo-1531891437562-4301cf35b7e4?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
        "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
        "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
        "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"
    };

    public static string Profile => Profiles[0];

    public static string GetProfile(string? uniqueKey)
    {
        if (string.IsNullOrWhiteSpace(uniqueKey))
        {
            return Profile;
        }

        var hash = 0;

        foreach (var character in uniqueKey.Trim())
        {
            hash = unchecked((hash * 31) + char.ToLowerInvariant(character));
        }

        var index = (int)((uint)hash % (uint)Profiles.Length);
        return Profiles[index];
    }
}
