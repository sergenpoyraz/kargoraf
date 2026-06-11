using System.Collections.ObjectModel;
using System.Windows.Input;
using KargoRaf.Commands;
using KargoRaf.Services;

namespace KargoRaf.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly SectionService _sectionService;
    private readonly BackupService _backupService;

    private string _statusMessage = string.Empty;
    private bool _canAddSection = true;

    public SettingsViewModel(SectionService sectionService, BackupService backupService)
    {
        _sectionService = sectionService;
        _backupService = backupService;
        Sections = new ObservableCollection<SectionEditItem>();

        SaveCommand = new RelayCommand(Save);
        AddSectionCommand = new RelayCommand(AddSection, () => CanAddSection);
        RemoveSectionCommand = new RelayCommand<SectionEditItem>(RemoveSection, item => item?.CanRemove == true);
        BackupCommand = new RelayCommand(BackupDatabase);
        ExportActiveCommand = new RelayCommand(ExportActive);
        ExportHistoryCommand = new RelayCommand(ExportHistory);

        Load();
    }

    public ObservableCollection<SectionEditItem> Sections { get; }

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

    public ICommand SaveCommand { get; }
    public ICommand AddSectionCommand { get; }
    public ICommand RemoveSectionCommand { get; }
    public ICommand BackupCommand { get; }
    public ICommand ExportActiveCommand { get; }
    public ICommand ExportHistoryCommand { get; }

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
            _backupService.CreateDatabaseBackup();
            StatusMessage = "Yedek alındı ✓";
        }
        catch (Exception ex)
        {
            StatusMessage = "Yedek alınamadı.";
            LoggingService.Instance.Error("Yedek alınamadı.", ex);
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
