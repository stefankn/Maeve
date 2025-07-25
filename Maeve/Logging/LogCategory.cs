using System.ComponentModel;
using Maeve.Extensions;

namespace Maeve.Logging;

public enum LogCategory {
    [Description("TOOLS"), ConsoleColor(Colors.BrightCyan)]
    Tools,
    
    [Description("DOCUMENTS"), ConsoleColor(Colors.Cyan)]
    Documents,
    
    [Description("LLM"), ConsoleColor(Colors.Magenta)]
    Llm,
    
    [Description("WEB"), ConsoleColor(Colors.Green)]
    Web
}

internal static class Colors {
    public const string Blue = "\u001b[44m";
    public const string Yellow = "\u001b[43m";
    public const string Green = "\u001b[42m";
    public const string Red = "\u001b[41m";
    public const string BrightRed = "\u001b[101m";
    public const string Cyan = "\u001b[46m";
    public const string BrightCyan = "\u001b[106m";
    public const string Magenta = "\u001b[105m";
}

[AttributeUsage(AttributeTargets.Field)]
public class ConsoleColorAttribute(string value) : Attribute {
    
    // - Properties

    public readonly string Value = value;
}

public static class LogCategoryExtensions {
    
    // - Functions

    public static string Description(this LogCategory category) {
        return category.GetDescriptionAttribute() ?? "";
    }

    public static string ConsoleColor(this LogCategory category) {
        var fieldInfo = category.GetType().GetField(category.ToString());
        if (fieldInfo == null) return Colors.Blue;
        
        var attributes = (ConsoleColorAttribute[])fieldInfo.GetCustomAttributes(typeof(ConsoleColorAttribute), false);
        return attributes.FirstOrDefault()?.Value ?? Colors.Blue;
    }
}