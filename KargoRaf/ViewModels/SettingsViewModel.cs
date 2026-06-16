using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using KargoRaf.Commands;
using KargoRaf.Helpers;
using KargoRaf.Services;

namespace KargoRaf.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly SectionService _sectionService;
    private readonly BackupService _backupService;
    private readonly PackageService _packageService;
    private readonly SettingsService _settingsService;

    private string _statusMessage = string.Empty;
    private bool _canAddSection = true;
    private UiDensity _selectedDensity = UiDensity.Compact;
    private bool _sectionAutoScrollEnabled = true;

    public SettingsViewModel(
        SectionService sectionService,
        BackupService backupService,
        PackageService packageService,
        SettingsService settingsService)
    {
        _sectionService = sectionService;
        _backupService = backupService;
        _packageService = packageService;
        _settingsService = settingsService;
        Sections = new ObservableCollection<SectionEditItem>();

        SaveCommand = new RelayCommand(Save);
        AddSectionCommand = new RelayCommand(AddSection, () => CanAddSection);
        RemoveSectionCommand = new RelayCommand<SectionEditItem>(RemoveSection, item => item?.CanRemove == true);
        BackupCommand = new RelayCommand(BackupDatabase);
        OpenBackupsFolderCommand = new RelayCommand(OpenBackupsFolder);
        ExportActiveCommand = new RelayCommand(ExportActive);
        ExportHistoryCommand = new RelayCommand(ExportHistory);
        DeleteAllPackagesCommand = new RelayCommand(DeleteAllPackages);

        Load();
    }

    public IList<DensitySettingItem> DensityItems { get; } = UiDensityCatalog.AllOptions
        .Select(option => new DensitySettingItem(option.Density, option.Profile))
        .ToList();

    public ObservableCollection<SectionEditItem> Sections { get; }

    public DensitySettingItem? SelectedDensityItem
    {
        get => DensityItems.FirstOrDefault(item => item.Density == SelectedDensity);
        set
        {
            if (value is null)
                return;
            SelectedDensity = value.Density;
        }
    }

    public UiDensity SelectedDensity
    {
        get => _selectedDensity;
        set
        {
            if (SetProperty(ref _selectedDensity, value))
            {
                OnPropertyChanged(nameof(SelectedDensityHint));
                OnPropertyChanged(nameof(SelectedDensityItem));
            }
        }
    }

    public string SelectedDensityHint =>
        UiDensityCatalog.GetProfile(SelectedDensity).Hint;

    public bool SectionAutoScrollEnabled
    {
        get => _sectionAutoScrollEnabled;
        set => SetProperty(ref _sectionAutoScrollEnabled, value);
    }

    public bool CanAddSection
    {
        get => _canAddSection;
        private set
        {
            if (SetProperty(ref _canAddSection, value))
                (AddSectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string DatabasePath => AppPaths.DatabasePath;

    public string BackupsFolderPath => AppPaths.BackupsFolder;

    public string BackupInfoSummary
    {
        get => _backupInfoSummary;
        private set => SetProperty(ref _backupInfoSummary, value);
    }

    private string _backupInfoSummary = string.Empty;

    public ICommand SaveCommand { get; }
    public ICommand AddSectionCommand { get; }
    public ICommand RemoveSectionCommand { get; }
    public ICommand BackupCommand { get; }
    public ICommand OpenBackupsFolderCommand { get; }
    public ICommand ExportActiveCommand { get; }
    public ICommand ExportHistoryCommand { get; }
    public ICommand DeleteAllPackagesCommand { get; }

    private void Load()
    {
        Sections.Clear();
        var all = _sectionService.GetActiveSections();
        foreach (var s in all)
        {
            var count = _sectionService.GetActivePackageCount(s.Id);
            Sections.Add(new SectionEditItem
            {
                Id = s.Id,
                SortOrder = s.SortOrder,
                Name = s.Name,
                ActivePackageCount = count,
                CanRemove = count == 0 && all.Count > SectionService.MinSections
            });
        }

        CanAddSection = all.Count < SectionService.MaxSections;
        SelectedDensity = UiDensityCatalog.Parse(_settingsService.Get(UiDensityCatalog.SettingsKey, "compact"));
        SectionAutoScrollEnabled = UiDensityCatalog.ParseAutoScroll(
            _settingsService.Get(UiDensityCatalog.AutoScrollSettingsKey, "true"));
        RefreshBackupInfo();
    }

    private void RefreshBackupInfo()
    {
        AppPaths.EnsureDirectories();
        var files = Directory.Exists(AppPaths.BackupsFolder)
            ? Directory.GetFiles(AppPaths.BackupsFolder, "*.backup.db")
            : [];

        if (files.Length == 0)
        {
            BackupInfoSummary = "Henüz yedek yok — «Veritabanı Yedeği Al» ile ilk yedeği oluşturun.";
            return;
        }

        var latest = files
            .Select(path => new FileInfo(path))
            .OrderByDescending(info => info.LastWriteTime)
            .First();

        BackupInfoSummary = files.Length == 1
            ? $"1 yedek dosyası · Son yedek: {latest.LastWriteTime:dd.MM.yyyy HH:mm}"
            : $"{files.Length} yedek dosyası · Son yedek: {latest.LastWriteTime:dd.MM.yyyy HH:mm}";
    }

    private void AddSection()
    {
        try
        {
            var section = _sectionService.AddSection();
            StatusMessage = $"'{section.Name}' eklendi ✓";
            Load();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            LoggingService.Instance.Error("Bölüm eklenemedi.", ex);
        }
    }

    private void RemoveSection(SectionEditItem? item)
    {
        if (item is null) return;
        try
        {
            _sectionService.DeactivateSection(item.Id);
            StatusMessage = $"'{item.Name}' kaldırıldı ✓";
            Load();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            LoggingService.Instance.Error("Bölüm kaldırılamadı.", ex);
        }
    }

    private void Save()
    {
        try
        {
            foreach (var item in Sections)
            {
                if (string.IsNullOrWhiteSpace(item.Name))
                {
                    StatusMessage = "Bölüm adları boş olamaz.";
                    return;
                }
                _sectionService.UpdateSectionName(item.Id, item.Name);
            }

            _settingsService.Set(UiDensityCatalog.SettingsKey, UiDensityCatalog.ToStorage(SelectedDensity));
            _settingsService.Set(
                UiDensityCatalog.AutoScrollSettingsKey,
                UiDensityCatalog.ToAutoScrollStorage(SectionAutoScrollEnabled));
            StatusMessage = "Kaydedildi ✓";
            Load();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Ayarlar kaydedilemedi.", ex);
            StatusMessage = "Kaydedilemedi — tekrar deneyin.";
        }
    }

    private void BackupDatabase()
    {
        try
        {
            var path = _backupService.CreateDatabaseBackup();
            RefreshBackupInfo();
            StatusMessage = $"Yedek alındı ✓  {Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Yedek alınamadı.";
            LoggingService.Instance.Error("Yedek alınamadı.", ex);
        }
    }

    private void OpenBackupsFolder()
    {
        try
        {
            AppPaths.EnsureDirectories();
            Process.Start(new ProcessStartInfo(AppPaths.BackupsFolder) { UseShellExecute = true });
            StatusMessage = "Yedek klasörü açıldı ✓";
        }
        catch (Exception ex)
        {
            StatusMessage = "Klasör açılamadı.";
            LoggingService.Instance.Error("Yedek klasörü açılamadı.", ex);
        }
    }

    private void DeleteAllPackages()
    {
        var result = MessageBox.Show(
            "Bu işlem aktif ve geçmişteki tüm kargo kayıtlarını kalıcı olarak silecek. " +
            "Bölüm ayarları korunur. Devam etmeden önce yedek almanız önerilir.\n\n" +
            "Devam etmek istiyor musunuz?",
            "Tüm Kargo Verilerini Sil",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            StatusMessage = "Silme işlemi iptal edildi.";
            return;
        }

        try
        {
            var deletedCount = _packageService.DeleteAllPackages();
            StatusMessage = deletedCount > 0
                ? $"{deletedCount} kargo kaydı silindi ✓"
                : "Silinecek kargo kaydı yok.";
            Load();
            WidgetViewModel.Instance?.Refresh();
        }
        catch (Exception ex)
        {
            StatusMessage = "Kargo verileri silinemedi.";
            LoggingService.Instance.Error("Tüm kargo verileri silinemedi.", ex);
        }
    }

    private void ExportActive()
    {
        try
        {
            if (_backupService.PromptAndExportActive())
                StatusMessage = "Aktif CSV dışa aktarıldı ✓";
        }
        catch (Exception ex)
        {
            StatusMessage = "CSV dışa aktarılamadı.";
            LoggingService.Instance.Error("CSV dışa aktarım hatası.", ex);
        }
    }

    private void ExportHistory()
    {
        try
        {
            if (_backupService.PromptAndExportHistory())
                StatusMessage = "Geçmiş CSV dışa aktarıldı ✓";
        }
        catch (Exception ex)
        {
            StatusMessage = "CSV dışa aktarılamadı.";
            LoggingService.Instance.Error("CSV dışa aktarım hatası.", ex);
        }
    }
}

public class SectionEditItem : ViewModelBase
{
    private string _name = string.Empty;
    private bool _canRemove;

    public int Id { get; set; }
    public int SortOrder { get; set; }
    public int ActivePackageCount { get; set; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public bool CanRemove
    {
        get => _canRemove;
        set => SetProperty(ref _canRemove, value);
    }

    public string Label => $"Bölüm {SortOrder}";

    public string PackageInfo => ActivePackageCount > 0
        ? $"{ActivePackageCount} aktif kargo"
        : "Boş";
}

public sealed class DensitySettingItem
{
    public DensitySettingItem(UiDensity density, UiDensityProfile profile)
    {
        Density = density;
        Title = $"{profile.Label} — {profile.CardWidth:0} px kart";
        Subtitle = profile.Hint;
    }

    public UiDensity Density { get; }
    public string Title { get; }
    public string Subtitle { get; }
}
