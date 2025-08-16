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
    
    public static string? GetKeyAttribute(this Enum value) {
        var fieldInfo = value.GetType().GetField(value.ToString());
        if (fieldInfo == null) return null;
        
        var attributes = (KeyAttribute[])fieldInfo.GetCustomAttributes(typeof(KeyAttribute), false);
        return attributes[0].Key;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class KeyAttribute(string key) : Attribute {
    
    // - Properties

    public readonly string Key = key;
}