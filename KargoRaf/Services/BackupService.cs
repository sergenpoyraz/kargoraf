using KargoRaf.Helpers;
using KargoRaf.Models;
using Microsoft.Win32;

namespace KargoRaf.Services;

public class BackupService
{
    private readonly PackageService _packageService;

    public BackupService(PackageService packageService)
    {
        _packageService = packageService;
    }

    public string CreateDatabaseBackup()
    {
        AppPaths.EnsureDirectories();
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupPath = Path.Combine(AppPaths.BackupsFolder, $"kargoraf_{timestamp}.backup.db");

        try
        {
            if (!File.Exists(AppPaths.DatabasePath))
                throw new FileNotFoundException("Veritabanı dosyası bulunamadı.");

            File.Copy(AppPaths.DatabasePath, backupPath, overwrite: true);
            LoggingService.Instance.Info($"Yedek alındı: {backupPath}");
            return backupPath;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Veritabanı yedeği alınamadı.", ex);
            throw;
        }
    }

    public void ExportActiveToCsv(string filePath)
    {
        ExportPackages(_packageService.GetActivePackages(), filePath, "Aktif");
    }

    public void ExportHistoryToCsv(string filePath, HistoryFilter filter = HistoryFilter.All)
    {
        ExportPackages(_packageService.GetDeliveredHistory(filter), filePath, "Geçmiş");
    }

    public bool PromptAndExportActive()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV dosyası (*.csv)|*.csv",
            FileName = $"aktif_kargolar_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };
        if (dialog.ShowDialog() != true) return false;
        ExportActiveToCsv(dialog.FileName);
        return true;
    }

    public bool PromptAndExportHistory(HistoryFilter filter = HistoryFilter.All)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV dosyası (*.csv)|*.csv",
            FileName = $"gecmis_kargolar_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };
        if (dialog.ShowDialog() != true) return false;
        ExportHistoryToCsv(dialog.FileName, filter);
        return true;
    }

    private static void ExportPackages(IEnumerable<Package> packages, string filePath, string kind)
    {
        try
        {
            var lines = new List<string>
            {
                "Id;Alıcı Adı;Bölüm;Not;Eklenme;Teslim"
            };

            foreach (var p in packages)
            {
                lines.Add(string.Join(';',
                    p.Id,
                    EscapeCsv(p.RecipientName),
                    EscapeCsv(p.SectionName),
                    EscapeCsv(p.Notes),
                    p.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    p.DeliveredAt?.ToString("yyyy-MM-dd HH:mm") ?? ""));
            }

            File.WriteAllLines(filePath, lines, System.Text.Encoding.UTF8);
            LoggingService.Instance.Info($"{kind} CSV dışa aktarıldı: {filePath}");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("CSV dışa aktarım başarısız.", ex);
            throw;
        }
    }

    private static string EscapeCsv(string value)
    {
        value = value.Replace("\"", "\"\"");
        return value.Contains(';') || value.Contains('"') ? $"\"{value}\"" : value;
    }
}
