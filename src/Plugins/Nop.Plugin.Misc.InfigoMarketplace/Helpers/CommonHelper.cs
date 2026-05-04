namespace Nop.Plugin.Misc.InfigoMarketplace.Helpers;

public static class CommonHelper
{
    public static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value ?? string.Empty;
        return value[..maxLength];
    }
}