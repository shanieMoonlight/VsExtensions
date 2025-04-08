namespace StronglyTypedAppSettings.Utils;
internal static class StringExtensions
{
    // Replaces the last occurrence of a substring in a string with another substring
    public static string ReplaceLastOccurrence(this string source, string find, string replace)
    {
        int place = source.LastIndexOf(find);
        if (place < 0)
            return source;
        return source.Remove(place, find.Length).Insert(place, replace);
    }
}