using System.ComponentModel;
using System.Text.RegularExpressions;
using Maeve.ModelProviders;

namespace Maeve.Extensions;

public static class StringExtensions {
    
    // - Functions
    
    public static string Truncate(this string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength] + "...";
    public static string DashToCamelCase(this string value) => Regex.Replace(value, "-.", m => m.Value.ToUpper()[1..]);

    public static Provider? AsProvider(this string value) {
        foreach (var field in typeof(Provider).GetFields()) {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is not DescriptionAttribute attribute)
                continue;
            
            if (attribute.Description == value) {
                return (Provider?)field.GetValue(null);
            }
        }

        return null;
    }
}