namespace Maeve.Extensions;

public static class StringExtensions {
    
    // - Functions
    
    public static string Truncate(this string value, int maxLength) {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength]; 
    }
}