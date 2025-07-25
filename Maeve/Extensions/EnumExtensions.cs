using System.ComponentModel;

namespace Maeve.Extensions;

public static class EnumExtensions {
    
    // - Functions

    public static string? GetDescriptionAttribute(this Enum value) {
        var fieldInfo = value.GetType().GetField(value.ToString());
        if (fieldInfo == null) return null;
        
        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes[0].Description;
    }
}