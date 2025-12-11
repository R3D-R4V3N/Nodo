namespace Rise.Shared.Assets
{
    public static class DefaultImages
    {
        private static readonly string[] Profiles =
        {
            "https://www.tenforums.com/geek/gars/images/2/types/thumb_15951118880user.png",
            "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
            "https://images.unsplash.com/photo-1544723795-3fb6469f5b39?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
            "https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
            "https://images.unsplash.com/photo-1531891437562-4301cf35b7e4?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
            "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
            "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
            "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"
        };

        public static string Profile => Profiles[0];

        // Inline SVG (base64 encoded)
        public static string Group =>
            "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI5NiIgaGVpZ2h0PSI5NiIgZmlsbD0iIzcyOTc4OCIgdmlld0JveD0iMCAwIDI1NiAyNTYiPjxwYXRoIGQ9Ik0xNjgsMTQ0YTQwLDQwLDAsMSwxLTQwLTQwQTQwLDQwLDAsMCwxLDE2OCwxNDRaTTY0LDU2QTMyLDMyLDAsMSwwLDk2LDg4LDMyLDMyLDAsMCwwLDY0LDU2Wm0xMjgsMGEzMiwzMiwwLDEsMCwzMiwzMkEzMiwzMiwwLDAsMCwxOTIsNTZaIiBvcGFjaXR5PSIwLjIiPjwvcGF0aD48cGF0aCBkPSJNMjQ0LjgsMTUwLjRhOCw4LDAsMCwxLTExLjItMS42QTUxLjYsNTEuNiwwLDAsMCwxOTIsMTI4YTgsOCwwLDAsMSwwLTE2LDI0LDI0LDAsMSwwLTIzLjI0LTMwLDgsOCwwLDEsMS0xNS41LTRBNDAsNDAsMCwxLDEsMjE5LDExNy41MWE2Ny45NCw2Ny45NCwwLDAsMSwyNy40MywyMS42OEE4LDgsMCwwLDEsMjQ0LjgsMTUwLjRaTTE5MC45MiwyMTJhOCw4LDAsMSwxLTEzLjg1LDgsNTcsNTcsMCwwLDAtOTguMTUsMCw4LDgsMCwxLDEtMTMuODQtOCw3Mi4wNiw3Mi4wNiwwLDAsMSwzMy43NC0yOS45Miw0OCw0OCwwLDEsMSw1OC4zNiwwQTcyLjA2LDcyLjA2LDAsMCwxLDE5MC45MiwyMTJaTTEyOCwxNzZhMzIsMzIsMCwxLDAtMzItMzJBMzIsMzIsMCwwLDAsMTI4LDE3NlpNNzIsMTIwYTgsOCwwLDAsMC04LThBMjQsMjQsMCwxLDEsODcuMjQsODJhOCw4LDAsMSwwLDE1LjUtNEE0MCw0MCwwLDEsMCwzNywxMTcuNTEsNjcuOTQsNjcuOTQsMCwwLDAsOS42LDEzOS4xOWE4LDgsMCwxLDAsMTIuOCw5LjYxQTUxLjYsNTEuNiwwLDAsMSw2NCwxMjgsOCw4LDAsMCwwLDcyLDEyMFoiPjwvcGF0aD48L3N2Zz4=";
        public static string GetProfile(string? uniqueKey)
        {
            if (string.IsNullOrWhiteSpace(uniqueKey))
                return Profile;

            var hash = 0;
            foreach (var c in uniqueKey.Trim())
                hash = unchecked((hash * 31) + char.ToLowerInvariant(c));

            var index = (int)((uint)hash % (uint)Profiles.Length);
            return Profiles[index];
        }

        public static string GetChatAvatar(bool isGroupChat, string? participantKey = null)
        {
            return isGroupChat ? Group : GetProfile(participantKey);
        }
    }
}
