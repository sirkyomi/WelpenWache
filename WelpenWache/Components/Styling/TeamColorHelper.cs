namespace WelpenWache.Components.Styling;

public static class TeamColorHelper
{
    public static string GetHueStyle(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        var normalized = key.Trim().ToUpperInvariant();
        uint hash = 2166136261;
        foreach (var ch in normalized)
        {
            hash ^= ch;
            hash *= 16777619;
        }

        var hue = (int)(hash % 360);
        return $"--team-hue:{hue};";
    }
}
