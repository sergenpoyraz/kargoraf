namespace KargoRaf.Services;

public class LoggingService
{
    private static readonly object Lock = new();
    private static LoggingService? _instance;
    public static LoggingService Instance => _instance ??= new LoggingService();

    private LoggingService()
    {
        Helpers.AppPaths.EnsureDirectories();
    }

    public void Info(string message) => Write("INFO", message);

    public void Warning(string message) => Write("WARN", message);

    public void Error(string message, Exception? ex = null)
    {
        var full = ex is null ? message : $"{message}{Environment.NewLine}{ex}";
        Write("ERROR", full);
    }

    private void Write(string level, string message)
    {
        try
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            lock (Lock)
            {
                File.AppendAllText(Helpers.AppPaths.LogFilePath, line + Environment.NewLine);
            }
        }
        catch
        {
            // Son çare: log yazılamazsa uygulamayı düşürme.
        }
    }
}
