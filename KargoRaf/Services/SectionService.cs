using Microsoft.Data.Sqlite;
using KargoRaf.Data;
using KargoRaf.Models;

namespace KargoRaf.Services;

public class SectionService
{
    public const int MaxSections = 12;
    public const int MinSections = 1;

    public event Action? SectionsChanged;

    public List<Section> GetActiveSections()
    {
        var list = new List<Section>();
        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT Id, Name, SortOrder, IsActive
                FROM Sections
                WHERE IsActive = 1
                ORDER BY SortOrder, Id
                """;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Section
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    SortOrder = reader.GetInt32(2),
                    IsActive = reader.GetInt32(3) == 1
                });
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Bölümler yüklenemedi.", ex);
            throw;
        }

        return list;
    }

    public Section AddSection(string? name = null)
    {
        var sections = GetActiveSections();
        if (sections.Count >= MaxSections)
            throw new InvalidOperationException($"En fazla {MaxSections} bölüm eklenebilir.");

        var nextOrder = sections.Count == 0 ? 1 : sections.Max(s => s.SortOrder) + 1;
        name = string.IsNullOrWhiteSpace(name) ? $"Bölüm {nextOrder}" : name.Trim();
        if (name.Length > 100) name = name[..100];

        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Sections (Name, SortOrder, IsActive)
                VALUES ($name, $sort, 1);
                SELECT last_insert_rowid();
                """;
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$sort", nextOrder);
            var id = Convert.ToInt32(cmd.ExecuteScalar());

            LoggingService.Instance.Info($"Yeni bölüm eklendi: {name}");
            SectionsChanged?.Invoke();
            return GetById(id)!;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Bölüm eklenemedi.", ex);
            throw;
        }
    }

    public void DeactivateSection(int id)
    {
        var sections = GetActiveSections();
        if (sections.Count <= MinSections)
            throw new InvalidOperationException($"En az {MinSections} bölüm kalmalı.");

        if (GetActivePackageCount(id) > 0)
            throw new InvalidOperationException("Bu bölümde aktif kargo var. Önce teslim edin veya taşıyın.");

        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Sections SET IsActive = 0 WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
            LoggingService.Instance.Info($"Bölüm kaldırıldı (Id={id}).");
            SectionsChanged?.Invoke();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"Bölüm kaldırılamadı (Id={id}).", ex);
            throw;
        }
    }

    public int GetActivePackageCount(int sectionId)
    {
        using var conn = SqliteConnectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*) FROM Packages
            WHERE SectionId = $id AND IsDelivered = 0
            """;
        cmd.Parameters.AddWithValue("$id", sectionId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void UpdateSectionName(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bölüm adı boş olamaz.");

        name = name.Trim();
        if (name.Length > 100) name = name[..100];

        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Sections SET Name = $name WHERE Id = $id";
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
            SectionsChanged?.Invoke();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"Bölüm güncellenemedi (Id={id}).", ex);
            throw;
        }
    }

    public Section? GetById(int id)
    {
        try
        {
            using var conn = SqliteConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT Id, Name, SortOrder, IsActive FROM Sections WHERE Id = $id
                """;
            cmd.Parameters.AddWithValue("$id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new Section
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                SortOrder = reader.GetInt32(2),
                IsActive = reader.GetInt32(3) == 1
            };
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"Bölüm okunamadı (Id={id}).", ex);
            throw;
        }
    }
}
