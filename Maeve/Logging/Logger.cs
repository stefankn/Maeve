using Serilog;

namespace Maeve.Logging;

public class Logger: ILogger {
    
    // - Private Properties

    private readonly Serilog.Core.Logger _logger;
    private readonly int _categoryPadLength;
    private readonly bool _isDevelopment;
    
    
    // - Construction

    public Logger(string serviceName, bool isDevelopment) {
        _isDevelopment = isDevelopment;
        _categoryPadLength = Enum.GetValues<LogCategory>()
            .Select(c => c.Description().Length)
            .Max();
        
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: $"data/logs/{serviceName}-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}"
            )
            .CreateLogger();
    }
    
    
    // - Functions

    public void Debug(string message, LogCategory category, bool consoleLog) {
        Log(message, LogLevel.Debug, category, consoleLog);
    }

    public void Information(string message, LogCategory category, bool consoleLog) {
        Log(message, LogLevel.Information, category, consoleLog);
    }

    public void Warning(string message, LogCategory category, bool consoleLog) {
        Log(message, LogLevel.Warning, category, consoleLog);
    }

    public void Error(string message, LogCategory category, bool consoleLog) {
        Log(message, LogLevel.Error, category, consoleLog);
    }

    public void Critical(string message, LogCategory category, bool consoleLog) {
        Log(message, LogLevel.Critical, category, consoleLog);
    }

    public void Object<T>(T obj, LogLevel level, LogCategory category, bool consoleLog) {
        Object(obj, level, category, consoleLog, 0);
    }
    
    
    // - Private Functions
    
    private void Log(string message, LogLevel level, LogCategory category, bool consoleLog = false) {
        string? overrideConsoleColor = null;

        switch (level) {
            case LogLevel.Information:
                _logger.Information("{Category} {Message}", PadCategory(category), message);
                break;
            case LogLevel.Warning:
                _logger.Warning("{Category} {Message}", PadCategory(category), message);
                overrideConsoleColor = Colors.Yellow;
                break;
            case LogLevel.Error:
                _logger.Error("{Category} {Message}", PadCategory(category), message);
                overrideConsoleColor = Colors.Red;
                break;
            case LogLevel.Critical:
                _logger.Fatal("{Category} {Message}", PadCategory(category), message);
                overrideConsoleColor = Colors.Red;
                break;
            default:
                _logger.Debug("{Category} {Message}", PadCategory(category), message);
                break;
        }

        if (consoleLog) {
            ConsoleLog(message, category, overrideConsoleColor);
        }
    }

    private void ConsoleLog(string message, LogCategory category, string? colorOverride = null) {
        if (_isDevelopment) {
            const string reset = "\u001b[0m";
            var color = colorOverride ?? category.ConsoleColor();
            Console.WriteLine($"{reset}{color} {category.Description(),-6} {reset} {message}");
        } else {
            Console.WriteLine($"{PadCategory(category)} {message}");
        }
    }

    private string PadCategory(LogCategory category) {
        var categoryString = $"[{category.Description()}]";
        return categoryString.PadRight(_categoryPadLength + 2, ' ');
    }

    private void Object<T>(T obj, LogLevel level, LogCategory category, bool consoleLog, int indentationLevel) {
        if (obj == null) return;
        
        var type = obj.GetType();

        if (indentationLevel == 0) {
            Log("", level, category, consoleLog);
            Log($"** {type} **", level, category, consoleLog);
            Log("", level, category, consoleLog);
        } else {
            var indentation = "|".PadRight(indentationLevel + 1);
            Log($"{indentation}{type}", level, category, consoleLog);
        }

        try {
            foreach (var propertyInfo in type.GetProperties()) {
                var value = propertyInfo.GetValue(obj);
                if (value != null && value.GetType().IsArray) {
                    var values = (object?[])value;
                    foreach (var o in values) {
                        Object(o, level, category, consoleLog, indentationLevel + 4);
                    }
                } else {
                    var separator = "|".PadRight(indentationLevel + 1);
                    Log($"{separator}  {propertyInfo.Name} = {value}", level, category, consoleLog);
                }
            }
        } catch {
            // Ignored
        }
        
        if (indentationLevel == 0) Log("", level, category, consoleLog);
    }
}