using Microsoft.Data.Sqlite;
using KargoRaf.Data;
using KargoRaf.Models;

namespace KargoRaf.Services;

public class PackageService
{
    public event Action? PackagesChanged;

    public Package Add(string recipientName, int sectionId, string notes = "")
    {
        recipientName = NormalizeName(recipientName);
        if (string.IsNullOrWhiteSpace(recipientName))
            throw new ArgumentException("Alıcı adı boş olamaz.");

        notes = (notes ?? string.Empty).Trim();
        if (notes.Length > 500) notes = notes[..500];

        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Packages (RecipientName, SectionId, Notes, CreatedAt, IsDelivered)
                VALUES ($name, $sectionId, $notes, $created, 0);
                SELECT last_insert_rowid();
                """;
            cmd.Parameters.AddWithValue("$name", recipientName);
            cmd.Parameters.AddWithValue("$sectionId", sectionId);
            cmd.Parameters.AddWithValue("$notes", notes);
            cmd.Parameters.AddWithValue("$created", DateTime.Now.ToString("O"));
            var id = Convert.ToInt32(cmd.ExecuteScalar());

            var package = GetById(id)!;
            PackagesChanged?.Invoke();
            LoggingService.Instance.Info($"Kargo eklendi: {recipientName} -> Bölüm {sectionId}");
            return package;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Kargo eklenemedi.", ex);
            throw;
        }
    }

    public bool ExistsActiveByName(string recipientName)
    {
        recipientName = NormalizeName(recipientName);
        if (string.IsNullOrWhiteSpace(recipientName)) return false;

        using var conn = SqliteConnectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*) FROM Packages
            WHERE IsDelivered = 0 AND RecipientName = $name COLLATE NOCASE
            """;
        cmd.Parameters.AddWithValue("$name", recipientName);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    public Package Deliver(int id)
    {
        try
        {
            var package = GetById(id)
                ?? throw new InvalidOperationException("Kargo bulunamadı.");

            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Packages WHERE Id = $id AND IsDelivered = 0";
            cmd.Parameters.AddWithValue("$id", id);
            if (cmd.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("Kargo teslim edilemedi.");

            PackagesChanged?.Invoke();
            LoggingService.Instance.Info($"Kargo teslim edildi: {package.RecipientName}");
            return package;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"Teslim işlemi başarısız (Id={id}).", ex);
            throw;
        }
    }

    public Package RestorePackage(Package package)
    {
        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Packages (Id, RecipientName, SectionId, Notes, CreatedAt, IsDelivered)
                VALUES ($id, $name, $sectionId, $notes, $created, 0)
                """;
            cmd.Parameters.AddWithValue("$id", package.Id);
            cmd.Parameters.AddWithValue("$name", package.RecipientName);
            cmd.Parameters.AddWithValue("$sectionId", package.SectionId);
            cmd.Parameters.AddWithValue("$notes", package.Notes ?? string.Empty);
            cmd.Parameters.AddWithValue("$created", package.CreatedAt.ToString("O"));
            cmd.ExecuteNonQuery();

            var restored = GetById(package.Id)!;
            PackagesChanged?.Invoke();
            LoggingService.Instance.Info($"Teslim geri alındı: {package.RecipientName}");
            return restored;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"Geri alma başarısız (Id={package.Id}).", ex);
            throw;
        }
    }


    public Package RestoreFromHistory(int id) =>
        throw new InvalidOperationException("Teslim edilen kayıtlar saklanmıyor.");

    public void Update(int id, string recipientName, int sectionId, string notes)
    {
        recipientName = NormalizeName(recipientName);
        if (string.IsNullOrWhiteSpace(recipientName))
            throw new ArgumentException("Alıcı adı boş olamaz.");

        notes = (notes ?? string.Empty).Trim();
        if (notes.Length > 500) notes = notes[..500];

        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                UPDATE Packages
                SET RecipientName = $name, SectionId = $sectionId, Notes = $notes
                WHERE Id = $id
                """;
            cmd.Parameters.AddWithValue("$name", recipientName);
            cmd.Parameters.AddWithValue("$sectionId", sectionId);
            cmd.Parameters.AddWithValue("$notes", notes);
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
            PackagesChanged?.Invoke();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"Kargo güncellenemedi (Id={id}).", ex);
            throw;
        }
    }

    public List<Package> GetActivePackages()
    {
        var list = new List<Package>();
        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT p.Id, p.RecipientName, p.SectionId, p.Notes, p.CreatedAt, p.DeliveredAt, p.IsDelivered, s.Name
                FROM Packages p
                INNER JOIN Sections s ON s.Id = p.SectionId
                WHERE p.IsDelivered = 0
                ORDER BY p.CreatedAt DESC
                """;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(ReadPackage(reader));
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Aktif kargolar yüklenemedi.", ex);
            throw;
        }

        return list;
    }

    public List<Package> GetActiveBySection(int sectionId)
    {
        return GetActivePackages().Where(p => p.SectionId == sectionId).ToList();
    }

    public List<Package> GetRecentActive(int count = 5)
    {
        return GetActivePackages().Take(count).ToList();
    }

    public List<Package> SearchActive(string query)
    {
        query = query.Trim();
        if (string.IsNullOrEmpty(query)) return GetActivePackages();

        var list = new List<Package>();
        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT p.Id, p.RecipientName, p.SectionId, p.Notes, p.CreatedAt, p.DeliveredAt, p.IsDelivered, s.Name
                FROM Packages p
                INNER JOIN Sections s ON s.Id = p.SectionId
                WHERE p.IsDelivered = 0
                  AND (p.RecipientName LIKE $q OR p.Notes LIKE $q)
                ORDER BY p.CreatedAt DESC
                """;
            cmd.Parameters.AddWithValue("$q", $"%{query}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(ReadPackage(reader));
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Arama başarısız.", ex);
            throw;
        }

        return list;
    }

    public List<Package> GetDeliveredHistory(HistoryFilter filter)
    {
        var list = new List<Package>();
        var (from, to) = GetFilterRange(filter);

        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT p.Id, p.RecipientName, p.SectionId, p.Notes, p.CreatedAt, p.DeliveredAt, p.IsDelivered, s.Name
                FROM Packages p
                INNER JOIN Sections s ON s.Id = p.SectionId
                WHERE p.IsDelivered = 1
                """;
            if (from.HasValue)
            {
                cmd.CommandText += " AND p.DeliveredAt >= $from";
                cmd.Parameters.AddWithValue("$from", from.Value.ToString("O"));
            }
            if (to.HasValue)
            {
                cmd.CommandText += " AND p.DeliveredAt < $to";
                cmd.Parameters.AddWithValue("$to", to.Value.ToString("O"));
            }
            cmd.CommandText += " ORDER BY p.DeliveredAt DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(ReadPackage(reader));
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Geçmiş yüklenemedi.", ex);
            throw;
        }

        return list;
    }

    public Package? GetById(int id)
    {
        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT p.Id, p.RecipientName, p.SectionId, p.Notes, p.CreatedAt, p.DeliveredAt, p.IsDelivered, s.Name
                FROM Packages p
                INNER JOIN Sections s ON s.Id = p.SectionId
                WHERE p.Id = $id
                """;
            cmd.Parameters.AddWithValue("$id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? ReadPackage(reader) : null;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"Kargo okunamadı (Id={id}).", ex);
            throw;
        }
    }

    public int GetActiveCount() =>
        GetActivePackages().Count;

    public Dictionary<int, int> GetCountsBySection(IEnumerable<int> sectionIds)
    {
        var counts = sectionIds.ToDictionary(id => id, _ => 0);
        foreach (var p in GetActivePackages())
        {
            if (counts.ContainsKey(p.SectionId))
                counts[p.SectionId]++;
        }
        return counts;
    }

    private static Package ReadPackage(SqliteDataReader reader)
    {
        return new Package
        {
            Id = reader.GetInt32(0),
            RecipientName = reader.GetString(1),
            SectionId = reader.GetInt32(2),
            Notes = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            CreatedAt = DateTime.Parse(reader.GetString(4)),
            DeliveredAt = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)),
            IsDelivered = reader.GetInt32(6) == 1,
            SectionName = reader.GetString(7)
        };
    }

    private static string NormalizeName(string name)
    {
        name = (name ?? string.Empty).Trim();
        if (name.Length > 120) name = name[..120];
        return name;
    }

    private static (DateTime? from, DateTime? to) GetFilterRange(HistoryFilter filter)
    {
        var today = DateTime.Today;
        return filter switch
        {
            HistoryFilter.Today => (today, today.AddDays(1)),
            HistoryFilter.Last7Days => (today.AddDays(-6), today.AddDays(1)),
            HistoryFilter.ThisMonth => (new DateTime(today.Year, today.Month, 1), today.AddMonths(1)),
            _ => (null, null)
        };
    }
}

public enum HistoryFilter
{
    All,
    Today,
    Last7Days,
    ThisMonth
}
