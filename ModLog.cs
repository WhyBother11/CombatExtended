using StardewModdingAPI;

namespace CombatExtended;

/// <summary>
/// Wrapper for <see cref="IMonitor"/> for convenient using.
/// </summary>
public class ModLog
{
    
    private readonly IMonitor _monitor;

    public ModLog(IMonitor monitor)
    {
        _monitor = monitor;
    }
    
    public void Trace(string message)
    {
        _monitor.Log(message, LogLevel.Trace);
    }
    
    public void Error(string message, Exception e)
    {
        var errorMsg = $"{message}\nException: {e}";
        _monitor.Log(errorMsg, LogLevel.Error);
    }

    public void Error(string message)
    {
        _monitor.Log(message, LogLevel.Error);
    }
    
    public void Info(string message)
    {
        _monitor.Log(message, LogLevel.Info);
    }

    public void Warn(string message)
    {
        _monitor.Log(message, LogLevel.Warn);
    }
}