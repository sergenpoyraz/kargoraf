using Microsoft.Data.Sqlite;
using KargoRaf.Data;

namespace KargoRaf.Services;

public class SettingsService
{
    public string Get(string key, string defaultValue = "")
    {
        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Value FROM Settings WHERE Key = $key";
            cmd.Parameters.AddWithValue("$key", key);
            var result = cmd.ExecuteScalar();
            return result is string s ? s : defaultValue;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"Ayar okunamadı: {key}", ex);
            return defaultValue;
        }
    }

    public void Set(string key, string value)
    {
        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Settings (Key, Value) VALUES ($key, $value)
                ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value
                """;
            cmd.Parameters.AddWithValue("$key", key);
            cmd.Parameters.AddWithValue("$value", value);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"Ayar kaydedilemedi: {key}", ex);
            throw;
        }
    }
}
