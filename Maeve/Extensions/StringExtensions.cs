using System.Text.RegularExpressions;

namespace Maeve.Extensions;

public static class StringExtensions {
    
    // - Functions
    
    public static string Truncate(this string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength] + "...";
    public static string DashToCamelCase(this string value) => Regex.Replace(value, "-.", m => m.Value.ToUpper()[1..]);
}