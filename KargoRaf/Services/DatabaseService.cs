using KargoRaf.Data;

namespace KargoRaf.Services;

public class DatabaseService
{
    public void Initialize() => DatabaseInitializer.Initialize();

    public bool TestConnection()
    {
        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            return conn.State == System.Data.ConnectionState.Open;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Veritabanı bağlantı testi başarısız.", ex);
            return false;
        }
    }
}
