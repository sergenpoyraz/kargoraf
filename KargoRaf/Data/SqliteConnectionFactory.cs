using Microsoft.Data.Sqlite;
using KargoRaf.Helpers;

namespace KargoRaf.Data;

public static class SqliteConnectionFactory
{
    public static SqliteConnection CreateConnection()
    {
        AppPaths.EnsureDirectories();
        var connection = new SqliteConnection($"Data Source={AppPaths.DatabasePath}");
        connection.Open();
        return connection;
    }
}
