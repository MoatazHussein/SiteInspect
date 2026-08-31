namespace SiteInspect.Application.Common.Persistence;

public static class RowVersionToken
{
    public static bool IsValid(string? value) => TryDecode(value, out _);

    public static byte[] Decode(string value) => Convert.FromBase64String(value);

    private static bool TryDecode(string? value, out byte[] rowVersion)
    {
        rowVersion = [];

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            rowVersion = Convert.FromBase64String(value);
            return rowVersion.Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
