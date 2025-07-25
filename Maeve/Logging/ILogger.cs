namespace Maeve.Logging;

public interface ILogger {
    
    // - Functions

    public void Debug(string message, LogCategory category, bool consoleLog = false);
    public void Information(string message, LogCategory category, bool consoleLog = false);
    public void Warning(string message, LogCategory category, bool consoleLog = false);
    public void Error(string message, LogCategory category, bool consoleLog = false);
    public void Critical(string message, LogCategory category, bool consoleLog = false);
    public void Object<T>(T obj, LogLevel level, LogCategory category, bool consoleLog = false);
}