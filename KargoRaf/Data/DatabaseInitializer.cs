using Microsoft.Data.Sqlite;
using KargoRaf.Services;

namespace KargoRaf.Data;

public static class DatabaseInitializer
{
    private static readonly string[] DefaultSections =
    [
        "Bölüm 1", "Bölüm 2", "Bölüm 3", "Bölüm 4", "Bölüm 5"
    ];

    public static void Initialize()
    {
        try
        {
            using var connection = SqliteConnectionFactory.CreateConnection();
            ExecuteNonQuery(connection, """
                CREATE TABLE IF NOT EXISTS Sections (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS Packages (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    RecipientName TEXT NOT NULL,
                    SectionId INTEGER NOT NULL,
                    Notes TEXT NOT NULL DEFAULT '',
                    CreatedAt TEXT NOT NULL,
                    DeliveredAt TEXT NULL,
                    IsDelivered INTEGER NOT NULL DEFAULT 0,
                    FOREIGN KEY (SectionId) REFERENCES Sections(Id)
                );

                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL DEFAULT ''
                );

                CREATE INDEX IF NOT EXISTS IX_Packages_SectionId ON Packages(SectionId);
                CREATE INDEX IF NOT EXISTS IX_Packages_IsDelivered ON Packages(IsDelivered);
                CREATE INDEX IF NOT EXISTS IX_Packages_RecipientName ON Packages(RecipientName);
                """);

            SeedDefaultSections(connection);
            LoggingService.Instance.Info("Veritabanı hazır.");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Veritabanı başlatılamadı.", ex);
            throw;
        }
    }

    private static void SeedDefaultSections(SqliteConnection connection)
    {
        using var countCmd = connection.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM Sections";
        var count = Convert.ToInt64(countCmd.ExecuteScalar());
        if (count > 0) return;

        for (var i = 0; i < DefaultSections.Length; i++)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Sections (Name, SortOrder, IsActive)
                VALUES ($name, $sort, 1)
                """;
            cmd.Parameters.AddWithValue("$name", DefaultSections[i]);
            cmd.Parameters.AddWithValue("$sort", i + 1);
            cmd.ExecuteNonQuery();
        }
    }

    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
