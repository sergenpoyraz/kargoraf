namespace KargoRaf.Helpers;

public static class AppPaths
{
    public static string AppDataFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KargoRaf");

    public static string DatabasePath => Path.Combine(AppDataFolder, "kargoraf.db");

    public static string LogsFolder => Path.Combine(AppDataFolder, "logs");

    public static string LogFilePath => Path.Combine(LogsFolder, "app.log");

    public static string BackupsFolder => Path.Combine(AppDataFolder, "backups");

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(AppDataFolder);
        Directory.CreateDirectory(LogsFolder);
        Directory.CreateDirectory(BackupsFolder);
    }
}
