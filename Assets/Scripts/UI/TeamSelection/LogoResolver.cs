using UnityEngine;

public static class LogoResolver
{
    private static bool _warnedDbMissing;

    private static string Norm(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = s.Trim().Replace("_","").Replace("-","").Replace(" ","");
        return s.ToUpperInvariant();
    }

    public static Sprite Get(string abbreviation)
    {
        var key = Norm(abbreviation);
        Sprite sprite = null;

        // Try DB first (if present)
        if (TeamLogoDatabase.Instance != null)
        {
            sprite = TeamLogoDatabase.Instance.Get(key);
        }
        else if (!_warnedDbMissing)
        {
            Debug.LogWarning("[Logo] TeamLogoDB.asset not found; using Resources fallback.");
            _warnedDbMissing = true;
        }

        // Fallbacks (your sprites are here)
        if (sprite == null)
            sprite = Resources.Load<Sprite>($"TeamSprites/{key}");
        if (sprite == null)
            sprite = Resources.Load<Sprite>($"TeamLogos/{key}"); // optional extra path

        return sprite;
    }
}
