namespace Maeve.Extensions;

public static class StringExtensions {
    
    // - Functions
    
    public static string Truncate(this string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength] + "...";
}