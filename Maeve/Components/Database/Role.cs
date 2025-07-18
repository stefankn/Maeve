namespace Maeve.Components.Database;

public enum Role {
    [Key("user")]
    User,
    
    [Key("assistant")]
    Assistant,
    
    [Key("system")]
    System,
    
    [Key("tool")]
    Tool
}

[AttributeUsage(AttributeTargets.Field)]
public class KeyAttribute(string value) : Attribute {
    
    // - Properties

    public readonly string Value = value;
}

public static class RoleExtensions {
    
    // - Functions

    public static string Key(this Role role) {
        var fieldInfo = role.GetType().GetField(role.ToString());
        if (fieldInfo == null) return "";
        
        var attributes = (KeyAttribute[])fieldInfo.GetCustomAttributes(typeof(KeyAttribute), false);
        return attributes.FirstOrDefault()?.Value ?? "";
    }
}